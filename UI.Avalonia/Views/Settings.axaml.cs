using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Avalonia.Views;

public partial class Settings : Window
{
    public Settings()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}