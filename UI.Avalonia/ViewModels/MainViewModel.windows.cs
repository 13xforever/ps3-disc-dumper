#if WINDOWS
#if USEWMI
using System;
using System.Management;

namespace UI.Avalonia.ViewModels;

public partial class MainViewModel
{
    ManagementEventWatcher? newDeviceWatcher, removedDeviceWatcher;

    public MainWindowViewModel()
    {
        if (!OperatingSystem.IsWindows())
            return;

#pragma warning disable CA1416
        newDeviceWatcher = new("select * from __InstanceCreationEvent within 1 where TargetInstance ISA 'Win32_CDROMDrive'");
        newDeviceWatcher.EventArrived += (_, _) => { ScanDiscsAsync().Wait(); };
        newDeviceWatcher.Start();

        removedDeviceWatcher = new("select * from __InstanceDeletionEvent within 1 where TargetInstance ISA 'Win32_CDROMDrive'");
        removedDeviceWatcher.EventArrived += (src, evt) =>
        {
            if (dumper is null)
                return;

            if (evt.NewEvent.Properties["TargetInstance"].Value is not ManagementBaseObject obj
                || obj.Properties["Drive"].Value is not string str
                || str[0] != dumper.Drive)
                return;
                
            dumper.Cts.Cancel();
            dumper = null;
            ResetViewModel();
        };
        removedDeviceWatcher.Start();
#pragma warning restore CA1416
    }

    partial void OnDisposePlatform()
    {
        if (!OperatingSystem.IsWindows())
            return;
        
        newDeviceWatcher?.Stop();
        newDeviceWatcher?.Dispose();
        removedDeviceWatcher?.Stop();
        removedDeviceWatcher?.Dispose();
    }
}
#endif
#endif