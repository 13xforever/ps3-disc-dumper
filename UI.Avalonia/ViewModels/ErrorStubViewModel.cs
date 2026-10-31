using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.Avalonia.ViewModels;

public partial class ErrorStubViewModel: ViewModelBase
{
    [ObservableProperty]
    public partial bool UacIsEnabled { get; set; } = false;

    public static string UacInfoLink => "https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/";
}