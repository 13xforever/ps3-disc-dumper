using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace UI.Avalonia.Views;

public partial class Settings : UserControl
{
    public Settings() => InitializeComponent();

    private void OnLoad(object? sender, RoutedEventArgs e)
    {
        var m = SettingsPanel.Margin;
        var w = this.FindAncestorOfType<Window>()!;
        var additionalTop = w.IsExtendedIntoWindowDecorations ? 16 : 50;
        SettingsPanel.Margin = new(m.Left, m.Top + additionalTop, m.Right, m.Bottom + 16);
    }
}