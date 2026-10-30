#if MACOS
using System;

namespace UI.Avalonia.Views;

public partial class ErrorStub
{
    partial void OnLoadedPlatform()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        TitleText.IsVisible = false;
    }
}
#endif
