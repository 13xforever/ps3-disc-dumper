#if MACOS
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using IrdLibraryClient;
using Ps3DiscDumper.Utils.MacOS;
using UI.Avalonia.ViewModels;
using CF = Ps3DiscDumper.Utils.MacOS.CoreFoundation;

namespace UI.Avalonia.Views;

public partial class MainWindow
{
    private Thread? diskArbiterThread;
    private IntPtr runLoop = IntPtr.Zero;

    partial void OnLoadedPlatform()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        // Finder opens applications with the root as the working directory.
        // In such cases, change it to the .app bundle directory so relative paths will work.
        if (Directory.GetCurrentDirectory() == "/")
        {
            Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")));
            Log.Debug($"Set working directory to: {Directory.GetCurrentDirectory()}");
        }

        diskArbiterThread = new(RunDiskArbiter);
        diskArbiterThread.Start();
    }

    partial void OnClosingPlatform()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        StopDiskArbiter();
        diskArbiterThread?.Join();
    }

    [SupportedOSPlatform("osx")]
    private unsafe void RunDiskArbiter()
    {
        try
        {
            runLoop = CF.CFRunLoopGetCurrent();
            var cfAllocator = CF.CFAllocatorGetDefault();
            var daSession = DiskArbitration.DASessionCreate(cfAllocator);
            var match = CF.CFDictionaryCreate(
                cfAllocator,
                [DiskArbitration.DescriptionMediaKindKey],
                [IOKit.BdMediaClassCfString],
                1,
                CF.TypeDictionaryKeyCallBacks,
                CF.TypeDictionaryValueCallBacks
            );
            DiskArbitration.DARegisterDiskAppearedCallback(daSession, match, &DiskAppeared, IntPtr.Zero);
            DiskArbitration.DARegisterDiskDisappearedCallback(daSession, match, &DiskDisappeared, IntPtr.Zero);
            DiskArbitration.DASessionScheduleWithRunLoop(daSession, runLoop, CF.RunLoopDefaultMode);

            // Blocks the thread until stopped.
            CF.CFRunLoopRun();

            DiskArbitration.DAUnregisterCallback(daSession, &DiskAppeared, IntPtr.Zero);
            DiskArbitration.DAUnregisterCallback(daSession, &DiskDisappeared, IntPtr.Zero);
            DiskArbitration.DASessionUnscheduleFromRunLoop(daSession, runLoop, CF.RunLoopDefaultMode);
            CF.CFRelease(daSession);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to run disk arbiter");
        }
    }

    [SupportedOSPlatform("osx")]
    private void StopDiskArbiter()
    {
        if (runLoop != IntPtr.Zero)
        {
            CF.CFRunLoopStop(runLoop);
        }
    }

    [SupportedOSPlatform("osx")]
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void DiskAppeared(IntPtr disk, IntPtr context)
    {
        try
        {
            var bsdName = Marshal.PtrToStringAnsi(DiskArbitration.DADiskGetBSDName(disk));
            Log.Debug($"Disk appeared: {bsdName}");
            // Delay before scanning as the drive may not be fully mounted yet.
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
                    {
                        MainWindow.DataContext: MainWindowViewModel
                        {
                            CurrentPage: MainViewModel
                            {
                                DumpingInProgress: false
                            } vm and not
                            {
                                // still scanning
                                FoundDisc: true,
                                DumperIsReady: false
                            }
                        }
                    })
                {
                    Log.Debug("Scanning for applicable disks");
                    vm.ScanDiscsCommand.Execute(null);
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception e)
        {
            Log.Error(e, "Unable to process disk appear event");
        }
    }

    [SupportedOSPlatform("osx")]
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void DiskDisappeared(IntPtr disk, IntPtr context)
    {
        try
        {
            var bsdName = Marshal.PtrToStringAnsi(DiskArbitration.DADiskGetBSDName(disk));
            Log.Debug($"Disk disappeared: {bsdName}");
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
                    {
                        MainWindow.DataContext: MainWindowViewModel
                        {
                            CurrentPage: MainViewModel
                            {
                                dumper: { SelectedPhysicalDevice: { Length: > 0 } spd } dumper
                            } vm
                        }
                    }
                    && spd == $"/dev/r{bsdName}")
                {
                    Log.Debug("Cancelling dump operation");
                    dumper.Cts.Cancel();
                    vm.ResetViewModelCommand.Execute(null);
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception e)
        {
            Log.Error(e, "Unable to process disk disappear event");
        }
    }
}
#endif
