#if LINUX
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

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
        
        // rounded corners setup
        if (w.FindAncestorOfType<Control>() is not { } host
            || host.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "PART_WindowBorder") is not {} windowBorder
            || host.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Name == "PART_CloseButton") is not {} windowCloseButton)
            return;

        var borderCornerRadius = windowBorder.CornerRadius;
        if (p.FindLogicalDescendantOfType<Border>() is { } bg)
        {
            bg.Margin = new(-1, -1, -1, -1); // panel stays put; only the card reaches the frame box
            bg.CornerRadius = borderCornerRadius;
            bg.BorderThickness = new(1);
            bg.BorderBrush = windowBorder.BorderBrush;
            windowBorder.GetObservable(Border.BorderBrushProperty).Subscribe(new WindowBorderBrushSync(bg));
        }

        /*
        if (p.FindLogicalDescendantOfType<ExperimentalAcrylicBorder>() is { } ab)
            ab.CornerRadius = borderCornerRadius;
        */
        foreach (var d in windowCloseButton.GetTemplateDescendants())
        {
            if (d is not Border b)
                continue;
            
            b.CornerRadius = new(0, borderCornerRadius.TopRight - 1, 0, 0);
            break;
        }

        // inactive windows border color changes
        w.Activated += (_, _) => windowBorder.ClearValue(Border.BorderBrushProperty);
        w.Deactivated += (_, _) => SetInactiveBorder(windowBorder);
        if (!w.IsActive)
            SetInactiveBorder(windowBorder);
    }
    
    private static void SetInactiveBorder(Border windowBorder)
    {
        if (windowBorder.FindResource("SurfaceStrokeColorDefaultBrush") is IBrush b)
            windowBorder.BorderBrush = b;
    }
    
    private sealed class WindowBorderBrushSync(Border target): IObserver<IBrush?>
    {
        public void OnCompleted() {}
        public void OnError(Exception error) {}
        public void OnNext(IBrush? value) => target.BorderBrush = value;
    }
}
#endif
