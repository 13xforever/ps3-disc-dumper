#if WINDOWS
using System;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace UI.Avalonia.ViewModels;

public partial class MainViewModel
{
    partial void ResetTaskbarProgress()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            return;

        try
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            TaskbarManager.Instance.SetProgressValue(0, ProgressMax);
        }
        catch (InvalidOperationException)
        {
        }
    }

    partial void EnableTaskbarProgress()
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
        }
        catch (InvalidOperationException)
        {
        }
    }

    partial void SetTaskbarProgress(int position)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                TaskbarManager.Instance.SetProgressValue(position, ProgressMax);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
#endif