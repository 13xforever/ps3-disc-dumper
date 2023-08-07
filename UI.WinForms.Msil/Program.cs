using System;
using System.Windows.Forms;
using Ps3DiscDumper.Utils;

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
        if (!SecurityEx.IsSafe(args))
        {
            MessageBox.Show("Please do not run software as Administrator unless application was designed to properly handle it, and it is explicitly required.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-1);
        }
#pragma warning disable CA1416
        Application.Run(new MainForm());
#pragma warning restore CA1416
    }
}