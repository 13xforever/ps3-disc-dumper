using System;
using System.Linq;
using System.Security.Principal;

namespace Ps3DiscDumper.Utils;

public static class SecurityEx
{
    public static bool IsSafe(params string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator)
                && !args.Any(p => p.Equals("/IUnderstandThatRunningSoftwareAsAdministratorIsDangerousAndNotRecommendedForAnyone")))
            {
                return false;
            }
        }
        return true;
    }
}