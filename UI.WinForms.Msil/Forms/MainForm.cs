using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Ps3DiscDumper;

namespace UI.WinForms.Msil
{
    public partial class MainForm : Form
    {
        private readonly Settings Settings = new Settings();
        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private readonly BackgroundWorker DiscDetectWorker = new BackgroundWorker();
        private readonly BackgroundWorker DiscDumpWorker = new BackgroundWorker();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Settings.Reload();
            if (!Settings.Configured)
            {
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                Settings.Reload();
            }
            productCodeLabel.Text = "";
            gameTitleLabel.Text = "";
            irdMatchLabel.Text = "";

            DiscDetectWorker.DoWork += DetectDisc;
            DiscDetectWorker.RunWorkerCompleted += DetectDiscFinished;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            DiscDetectWorker.RunWorkerAsync(new Dumper());
        }

        private async void DetectDisc(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                await dumper.DetectDiscAsync(Settings.OutputDir, Settings.IrdDir, Cts.Token).ConfigureAwait(false);
            }
            catch { }
            doWorkEventArgs.Result = dumper;
        }


        private void DetectDiscFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            if (!string.IsNullOrEmpty(dumper.ProductCode))
            {
                productCodeLabel.Text = dumper.ProductCode;
                gameTitleLabel.Text = dumper.Title;
                irdMatchLabel.Text = dumper.IrdFilename ?? "No match found!";
            }
        }
    }
}
