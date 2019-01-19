using System;
using System.ComponentModel;
using System.IO;
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

            ResetForm();

            DiscDetectWorker.DoWork += DetectDisc;
            DiscDetectWorker.RunWorkerCompleted += DetectDiscFinished;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            DiscDetectWorker.RunWorkerAsync(new Dumper());
        }

        private void ResetForm()
        {
            productCodeLabel.Text = "";
            gameTitleLabel.Text = "";
            irdMatchLabel.Text = "";

            step1StatusLabel.Text = "⏳";
            step2StatusLabel.Text = "";
            step3StatusLabel.Text = "";
            step4StatusLabel.Text = "";

            step1Label.Text = "Insert PS3 game disc";
            step2Label.Text = "Select matching IRD file";
            step3Label.Text = "Decrypt and copy files";
            step4Label.Text = "Validate integrity";

            step2Label.Enabled = false;
            step3Label.Enabled = false;
            step4Label.Enabled = false;
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
                step1StatusLabel.Text = "✔";
                step2StatusLabel.Text = "⏳";
                step2Label.Enabled = true;

                productCodeLabel.Text = dumper.ProductCode;
                gameTitleLabel.Text = dumper.Title;
                //irdMatchLabel.Text = string.IsNullOrEmpty(dumper.IrdFilename) ? "❌" : "✔";
            }

            if (string.IsNullOrEmpty(dumper.IrdFilename))
            {
                irdMatchLabel.Text = "No match found";
                step2StatusLabel.Text = "❌";
            }
            else
            {
                step2StatusLabel.Text = "✔";
                step3StatusLabel.Text = "⏳";
                step3Label.Enabled = true;
                irdMatchLabel.Text = Path.GetFileNameWithoutExtension(dumper.IrdFilename);
            }
        }
    }
}
