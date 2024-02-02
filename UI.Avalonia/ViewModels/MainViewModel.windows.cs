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
            //ignore in design mode
        }
    }

    partial void EnableTaskbarProgress()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
    }

    partial void SetTaskbarProgress(int position)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            TaskbarManager.Instance.SetProgressValue(position, ProgressMax);
    }
}
#endif