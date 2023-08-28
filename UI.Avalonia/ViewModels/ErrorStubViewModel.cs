using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.Avalonia.ViewModels;

public partial class ErrorStubViewModel: ViewModelBase
{
    [ObservableProperty] private bool uacIsEnabled = false;
    public string UacInfoLink => "https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/";
}