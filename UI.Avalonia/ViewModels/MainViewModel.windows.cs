#if WINDOWS
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using TerraFX.Interop.Windows;
using UI.Avalonia.Utils;
using ITaskbarList3 = TerraFX.Interop.Windows.ITaskbarList3;

namespace UI.Avalonia.ViewModels;

public unsafe partial class MainViewModel
{
    private static readonly Shobj shobj = new();
    
    private bool TryGetMainHandle(out HWND hwnd)
    {
        hwnd = default;
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: Window w }
            || w.TryGetPlatformHandle() is not { } platformHandle)
            return false;
        
        hwnd = (HWND)platformHandle.Handle;
        return true;
    }
    
    partial void ResetTaskbarProgress()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)
            || !TryGetMainHandle(out var hwnd))
            return;

        try
        {
            shobj.Taskbar->SetProgressState(hwnd, TBPFLAG.TBPF_NOPROGRESS);
            shobj.Taskbar->SetProgressValue(hwnd, 0ul, (ulong)ProgressMax);
        }
        catch (InvalidOperationException)
        {
            //ignore in design mode
        }
    }

    partial void EnableTaskbarProgress()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1)
            && TryGetMainHandle(out var hwnd))
            shobj.Taskbar->SetProgressState(hwnd, TBPFLAG.TBPF_NORMAL);
    }

    partial void SetTaskbarProgress(int position)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1)
            && TryGetMainHandle(out var hwnd))
            shobj.Taskbar->SetProgressValue(hwnd, (ulong)position, (ulong)ProgressMax);
    }
}
#endif