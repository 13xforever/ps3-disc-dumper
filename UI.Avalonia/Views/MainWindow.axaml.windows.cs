#if WINDOWS
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Threading;
using TerraFX.Interop.Windows;
using UI.Avalonia.Utils;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia.Views;

public partial class MainWindow
{
    // see https://learn.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-dev_broadcast_hdr
    private const int DBT_DEVNODES_CHANGED = 0x0007;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVEPENDING = 0x8003;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

    private const int DBT_DEVTYP_VOLUME = 0x2;
    private const int DBTF_MEDIA = 0x0001;
    private const int DBTF_MOUNT_ISO = 0x001b0000; // ???????

    partial void OnLoadedPlatform()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            WndProcHelper.Register(this, WndProc);
    }
    
    [SupportedOSPlatform("Windows6.1")]
    private void WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        if (DataContext is not MainWindowViewModel {CurrentPage: MainViewModel vm})
            return;

        if (msg != WM.WM_DEVICECHANGE)
            return;

#if DEBUG
        Debug.WriteLine($"WndProc msg 0x{msg:x4}, wParam 0x{wParam:x4}, lParam 0x{lParam:x4}");
#endif
        var msgType = (int)wParam;
        if (msgType is not (DBT_DEVICEARRIVAL or DBT_DEVICEREMOVEPENDING or DBT_DEVICEREMOVECOMPLETE))
            return;

        var hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR))!;
#if DEBUG
        Debug.WriteLine($"hdr: devicetype 0x{hdr.dbch_devicetype:x4}, reserved 0x{hdr.dbch_reserved:x4}");
#endif
        if (hdr.dbch_devicetype is not DBT_DEVTYP_VOLUME)
            return;

        var vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME))!;
#if DEBUG
        Debug.WriteLine($"dbcv: devicetype 0x{vol.dbcv_devicetype:x4}, unitmask 0x{vol.dbcv_unitmask:x4}, flags 0x{vol.dbcv_flags:x8}");
#endif
        if (msgType == DBT_DEVICEARRIVAL) //&& (vol.dbcv_flags & (DBTF_MEDIA | DBTF_MOUNT_ISO)) != 0
            Dispatcher.UIThread.Post(() => vm.ScanDiscsCommand.Execute(null), DispatcherPriority.Background); 
        else if (msgType is DBT_DEVICEREMOVEPENDING or DBT_DEVICEREMOVECOMPLETE)
        {
            var dumper = vm.dumper;
            var driveId = dumper?.Drive.ToDriveId() ?? 0;
            if ((vol.dbcv_unitmask & driveId) == 0)
                return;

            dumper?.Cts.Cancel();
            Dispatcher.UIThread.Post(() => vm.ResetViewModelCommand.Execute(null));
        }
    }
}
#endif