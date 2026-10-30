using System;
using System.Linq;
using System.Security.Principal;

namespace Ps3DiscDumper.Utils;

public static class SecurityEx
{
    public static bool IsSafe(params string[]? args)
    {
        if (!OperatingSystem.IsWindows())
            return true;
        
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            return true;

        if (args is null)
            return true;
        
        return args.Any(
            p => p.Equals(
                "/IUnderstandThatRunningSoftwareAsAdministratorIsDangerousAndNotRecommendedForAnyone",
                StringComparison.OrdinalIgnoreCase
            )
        );
    }
}