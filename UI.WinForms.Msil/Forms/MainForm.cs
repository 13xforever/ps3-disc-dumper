using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.WinForms.Msil.Utils;

namespace UI.WinForms.Msil
{
    public partial class MainForm : Form
    {
        private readonly Settings settings = new Settings();
        private BackgroundWorker discBackgroundWorker;
        private Dumper currentDumper;

        private const int WM_DEVICECHANGE = 0x219;

        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private const int DBT_DEVTYP_VOLUME = 0x2;
        private const int DBTF_MEDIA = 0x0001;
        private const int DBTF_MOUNT_ISO = 0x001b0000; // ???????

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);
            if (msg.Msg == WM_DEVICECHANGE)
            {
                var msgType = msg.WParam.ToInt32();
                switch (msgType)
                {
/*
                    case DBT_DEVNODES_CHANGED:
                        DetectPhysicalDiscStatus();
                        break;
*/
                    case DBT_DEVICEARRIVAL:
                    case DBT_DEVICEREMOVECOMPLETE:
                        var hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_HDR));
                        if (hdr.dbch_devicetype == DBT_DEVTYP_VOLUME)
                        {
                            var vol = (DEV_BROADCAST_VOLUME)msg.GetLParam(typeof(DEV_BROADCAST_VOLUME));
                            if ((vol.dbcv_flags & (DBTF_MEDIA | DBTF_MOUNT_ISO)) != 0)
                            {
                                if (msgType == DBT_DEVICEARRIVAL)
                                {
                                    if (!discBackgroundWorker.IsBusy)
                                        DetectPhysicalDiscStatus();
                                }
                                else
                                {
                                    var dumper = currentDumper;
                                    var driveId = dumper.Drive.ToDriveId();
                                    if ((vol.dbcv_unitmask & driveId) != 0)
                                    {
                                        if (discBackgroundWorker.IsBusy && !discBackgroundWorker.CancellationPending)
                                        {
                                            dumper.Cts.Cancel();
                                            discBackgroundWorker.CancelAsync();
                                        }
                                        ResetForm();
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            settings.Reload();
            if (!settings.Configured)
            {
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                settings.Reload();
            }

            ResetForm();
        }

        private void MainForm_Shown(object sender, EventArgs e) => DetectPhysicalDiscStatus();

        private void ResetForm()
        {
            productCodeLabel.Text = "";
            gameTitleLabel.Text = "";
            irdMatchLabel.Text = "";
            discSizeLabel.Text = "";

            step1StatusLabel.Text = "▶";
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

            discBackgroundWorker?.Dispose();
            discBackgroundWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            discBackgroundWorker.DoWork += DetectPs3DiscGame;
            discBackgroundWorker.RunWorkerCompleted += DetectPs3DiscGameFinished;
        }

        private void DetectPhysicalDiscStatus()
        {
            step1Label.Text = "Checking inserted disc...";
            currentDumper = new Dumper(new CancellationTokenSource());
            discBackgroundWorker.RunWorkerAsync(currentDumper);
        }

        private void DetectPs3DiscGame(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                dumper.DetectDisc();
            }
            catch { }
            doWorkEventArgs.Result = dumper;
        }

        private void DetectPs3DiscGameFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(dumper.ProductCode))
            {
                ResetForm();
                return;
            }

            step1StatusLabel.Text = "✔";
            step1Label.Text = "PS3 game disc detected";
            step2StatusLabel.Text = "⏳";
            step2Label.Enabled = true;
            step2Label.Text = "Looking for matching IRD file...";

            productCodeLabel.Text = dumper.ProductCode;
            gameTitleLabel.Text = dumper.Title;
            discSizeLabel.Text = dumper.TotalFileSize.AsStorageUnit();
            //irdMatchLabel.Text = string.IsNullOrEmpty(dumper.IrdFilename) ? "❌" : "✔";

            discBackgroundWorker.DoWork -= DetectPs3DiscGame;
            discBackgroundWorker.RunWorkerCompleted -= DetectPs3DiscGameFinished;
            discBackgroundWorker.DoWork += FindMatchingIrd;
            discBackgroundWorker.RunWorkerCompleted += FindMatchingIrdFinished;
            discBackgroundWorker.RunWorkerAsync(dumper);
        }

        private void FindMatchingIrd(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                dumper.FindIrdAsync(settings.OutputDir, settings.IrdDir).Wait(dumper.Cts.Token);
            }
            catch (Exception e)
            {
//                MessageBox.Show(e.Message, "Disc check error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            doWorkEventArgs.Result = dumper;
        }

        private void FindMatchingIrdFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(dumper.IrdFilename))
            {
                irdMatchLabel.Text = "No match found";
                step2StatusLabel.Text = "❌";
                step2Label.Text = "Select matching IRD file...";
            }
            else
            {
                step2StatusLabel.Text = "✔";
                step2Label.Text = "Matched IRD file selected";
                step3StatusLabel.Text = "▶";
                step3Label.Text = "Start disc decryption...";
                step3Label.Enabled = true;
                irdMatchLabel.Text = Path.GetFileNameWithoutExtension(dumper.IrdFilename);
            }
            discBackgroundWorker.DoWork -= FindMatchingIrd;
            discBackgroundWorker.RunWorkerCompleted -= FindMatchingIrdFinished;
        }
    }
}
