#if MACOS
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia.Threading;
using IrdLibraryClient;
using Ps3DiscDumper.Utils.MacOS;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia.Views;

public partial class MainWindow
{
    private Thread? diskArbiterThread;

    private IntPtr runLoop = IntPtr.Zero;

    partial void OnLoadedPlatform()
    {
        if (!OperatingSystem.IsMacOS())
            return;

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

    [SupportedOSPlatform("OSX")]
    private void RunDiskArbiter()
    {
        try
        {
            runLoop = CoreFoundation.CFRunLoopGetCurrent();

            var diskAppearedDelegate = new DiskArbitration.DADiskAppearedCallback(DiskAppeared);
            var diskDisappearedDelegate = new DiskArbitration.DADiskDisappearedCallback(DiskDisappeared);
            var diskAppearedDelegatePtr = Marshal.GetFunctionPointerForDelegate(diskAppearedDelegate);
            var diskDisappearedDelegatePtr = Marshal.GetFunctionPointerForDelegate(diskDisappearedDelegate);

            var cfAllocator = CoreFoundation.CFAllocatorGetDefault();
            var daSession = DiskArbitration.DASessionCreate(cfAllocator);
            var match = CoreFoundation.CFDictionaryCreate(cfAllocator,
                [DiskArbitration.DescriptionMediaKindKey], [IOKit.BDMediaClassCFString], 1,
                CoreFoundation.TypeDictionaryKeyCallBacks, CoreFoundation.TypeDictionaryValueCallBacks);
            DiskArbitration.DARegisterDiskAppearedCallback(daSession, match, diskAppearedDelegatePtr, IntPtr.Zero);
            DiskArbitration.DARegisterDiskDisappearedCallback(daSession, match, diskDisappearedDelegatePtr, IntPtr.Zero);
            DiskArbitration.DASessionScheduleWithRunLoop(daSession, runLoop, CoreFoundation.RunLoopDefaultMode);

            // Blocks the thread until stopped.
            CoreFoundation.CFRunLoopRun();

            DiskArbitration.DAUnregisterCallback(daSession, diskAppearedDelegatePtr, IntPtr.Zero);
            DiskArbitration.DAUnregisterCallback(daSession, diskDisappearedDelegatePtr, IntPtr.Zero);
            DiskArbitration.DASessionUnscheduleFromRunLoop(daSession, runLoop, CoreFoundation.RunLoopDefaultMode);
            CoreFoundation.CFRelease(daSession);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to run disk arbiter");
        }
    }

    [SupportedOSPlatform("OSX")]
    private void StopDiskArbiter()
    {
        if (runLoop != IntPtr.Zero)
        {
            CoreFoundation.CFRunLoopStop(runLoop);
        }
    }

    [SupportedOSPlatform("OSX")]
    private void DiskAppeared(IntPtr disk, IntPtr context)
    {
        try
        {
            var bsdName = Marshal.PtrToStringAnsi(DiskArbitration.DADiskGetBSDName(disk));
            Log.Debug($"Disk appeared: {bsdName}");
            // Delay before scanning as the drive may not be fully mounted yet.
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is MainWindowViewModel
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

    [SupportedOSPlatform("OSX")]
    private void DiskDisappeared(IntPtr disk, IntPtr context)
    {
        try
        {
            var bsdName = Marshal.PtrToStringAnsi(DiskArbitration.DADiskGetBSDName(disk));
            Log.Debug($"Disk disappeared: {bsdName}");
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is MainWindowViewModel
                    {
                        CurrentPage: MainViewModel
                        {
                            dumper: { SelectedPhysicalDevice: { Length: > 0 } spd } dumper
                        } vm
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
