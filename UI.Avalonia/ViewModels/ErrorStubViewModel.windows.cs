#if WINDOWS
using System;
using System.Security.AccessControl;
using IrdLibraryClient;
using Microsoft.Win32;

namespace UI.Avalonia.ViewModels;

public partial class ErrorStubViewModel: ViewModelBase
{

    public ErrorStubViewModel()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                RegistryRights.ReadKey | RegistryRights.QueryValues
            );
            UacIsEnabled = key?.GetValue("EnableLUA") is 1;
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to check UAC status");
        }
    }
}
#endif