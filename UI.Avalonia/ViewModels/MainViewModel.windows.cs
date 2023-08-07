#if WINDOWS
using System.Runtime.Versioning;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace UI.Avalonia.ViewModels;

public partial class MainViewModel
{
    partial void ResetTaskbarProgress()
    {
        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        TaskbarManager.Instance.SetProgressValue(0, ProgressMax);
    }

    partial void EnableTaskbarProgress()
    {
        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
    }

    partial void SetTaskbarProgress(int position)
    {
        TaskbarManager.Instance.SetProgressValue(position, ProgressMax);
    }
}
#endif