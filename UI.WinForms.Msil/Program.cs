using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UI.WinForms.Msil;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator)
                && !args.Any(p => p.Equals("/IUnderstandThatRunningSoftwareAsAdministratorIsDangerousAndNotRecommendedForAnyone")))
            {
                MessageBox.Show("Please do not run software as Administrator unless application was designed to properly handle it, and it is explicitly required.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
        Application.Run(new MainForm());
    }
}