#if LINUX
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;

namespace UI.Avalonia;

public partial class App
{
    partial void OnLoadedPlatform(object? sender, RoutedEventArgs e)
    {
        // check if we're running on Wayland with CSD enabled
        if (!OperatingSystem.IsLinux()
            || sender is not Window { WindowDecorationMargin: { Left: >0, Right: >0 } wm } w)
            return;
        
        w.Width += wm.Left + wm.Right;
        w.Height += wm.Left + wm.Bottom;
        if (w.FindControl<Panel>("TopWindowPanel") is not Control p)
            return;

        p.Margin = new(wm.Left, wm.Left, wm.Right, wm.Bottom);
    }
}
#endif
