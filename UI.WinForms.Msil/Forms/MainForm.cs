using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Microsoft.WindowsAPICodePack.Taskbar;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.WinForms.Msil.Utils;

namespace UI.WinForms.Msil
{
    public partial class MainForm : Form
    {
        private readonly Settings settings = new();
        private BackgroundWorker discBackgroundWorker;
        private Dumper currentDumper;

        private const int WM_DEVICECHANGE = 0x219;

        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private const int DBT_DEVTYP_VOLUME = 0x2;
        private const int DBTF_MEDIA = 0x0001;
        private const int DBTF_MOUNT_ISO = 0x001b0000; // ???????

        private static readonly NameValueCollection RegionMapping = new()
        {
            ["A"] = "ASIA",
            ["E"] = "EU",
            ["H"] = "HK",
            ["J"] = "JP",
            ["K"] = "KR",
            ["P"] = "JP",
            ["T"] = "JP",
            ["U"] = "US",
        };

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
                                    rescanDiscsButton_Click(null, null);
                                }
                                else
                                {
                                    var dumper = currentDumper;
                                    var driveId = dumper?.Drive.ToDriveId() ?? 0;
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
            Text = "PS3 Disc Dumper v" + Dumper.Version;
            Log.Info(Text);
            settings.Reload();
            ResetForm();
        }


        private void settingsButton_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
            settings.Reload();
        }

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
            step2Label.Text = "Select disc key file";
            step3Label.Text = "Decrypt and copy files";
            step4Label.Text = "Validate integrity";

            step2Label.Enabled = false;
            step3Label.Enabled = false;
            step4Label.Enabled = false;

            currentDumper = null;
            discBackgroundWorker?.CancelAsync();
            discBackgroundWorker?.Dispose();
            discBackgroundWorker = new() { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            discBackgroundWorker.DoWork += DetectPs3DiscGame;
            discBackgroundWorker.RunWorkerCompleted += DetectPs3DiscGameFinished;

            settingsButton.Enabled = true;
            rescanDiscsButton.Enabled = true;
            rescanDiscsButton.Visible = true;
            selectIrdButton.Visible = false;
            selectIrdButton.Enabled = false;
            startDumpingButton.Visible = false;
            startDumpingButton.Enabled = false;
            cancelDiscDumpButton.Visible = false;
            cancelDiscDumpButton.Enabled = false;
            dumpingProgressLabel.Visible = false;
            dumpingProgressLabel.Text = "";
            dumpingProgressBar.Visible = false;
            dumpingProgressBar.Value = 0;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, Handle);
            TaskbarManager.Instance.SetProgressValue(0, dumpingProgressBar.Maximum, Handle);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            rescanDiscsButton_Click(sender, e);
        }

        private void rescanDiscsButton_Click(object sender, EventArgs e)
        {
            if (!discBackgroundWorker.IsBusy && currentDumper == null)
                DetectPhysicalDiscStatus();
        }

        private void selectIrdButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = ".ird",
                Filter = "IRD file (*.ird)|*.ird|Redump disc key file (*.dkey)|*.dkey|All supported files|*.ird;*.dkey|All files|*",
                FilterIndex = 2,
                Title = "Select a disc key file",
                SupportMultiDottedExtensions = true,
                InitialDirectory = settings.IrdDir,
            };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != DialogResult.OK || string.IsNullOrEmpty(dialog.FileName) || !File.Exists(dialog.FileName))
                return;

            var discKeyPath = dialog.FileName;
            try
            {
                var discKey = File.ReadAllBytes(discKeyPath);
                DiscKeyInfo keyInfo;
                if (discKey.Length > 256 / 8)
                {
                    var ird = IrdParser.Parse(discKey);
                    keyInfo = new(ird.Data1, null, discKeyPath, KeyType.Ird, ird.Crc32.ToString("x8"));
                }
                else
                    keyInfo = new(null, discKey, discKeyPath, KeyType.Redump, discKey.ToHexString());
                var discKeyFilename = Path.GetFileName(discKeyPath);
                var cacheFilename = Path.Combine(settings.IrdDir, discKeyFilename);
                if (!File.Exists(cacheFilename))
                    File.Copy(discKeyPath, cacheFilename);

                //todo: proper check
                currentDumper.FindDiscKeyAsync(settings.IrdDir).GetAwaiter().GetResult();
                if (!currentDumper.IsValidDiscKey(discKey))
                {
                    MessageBox.Show("Selected disk key file contains incompatible file set, and cannot be used with the selected PS3 game disc.", "IRD file check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectIrdButton.Visible = false;
                selectIrdButton.Enabled = false;
                FindMatchingIrdFinished(sender, new(currentDumper, null, currentDumper?.Cts.IsCancellationRequested ?? true));
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to check IRD");
                MessageBox.Show(ex.Message, "IRD Check Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void startDumpingButton_Click(object sender, EventArgs e)
        {
            var outputDir = Path.Combine(settings.OutputDir, currentDumper.OutputDir);
            if (Directory.Exists(outputDir))
            {
                var msgResult = MessageBox.Show(
                    $"Output folder ({currentDumper.OutputDir}) already exists.\n" +
                    "Are you sure you want to overwrite any existing files?",
                    "Output folder check",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (msgResult == DialogResult.No)
                    return;
            }

            settingsButton.Enabled = false;
            startDumpingButton.Enabled = false;
            startDumpingButton.Visible = false;
            step3StatusLabel.Text = "⏳";
            step3Label.Text = "Decrypting and copying files...";


            discBackgroundWorker.DoWork += DumpDisc;
            discBackgroundWorker.RunWorkerCompleted += DumpDiscFinished;
            discBackgroundWorker.ProgressChanged += DiscDumpUpdateProgress;
            discBackgroundWorker.RunWorkerAsync(currentDumper);

            cancelDiscDumpButton.Enabled = true;
            cancelDiscDumpButton.Visible = true;
            dumpingProgressBar.Visible = true;
            dumpingProgressLabel.Text = "Analyzing file structure...";
            dumpingProgressLabel.Visible = true;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal, Handle);
        }

        private void cancelDiscDumpButton_Click(object sender, EventArgs e)
        {
            discBackgroundWorker.CancelAsync();
            currentDumper.Cts.Cancel();
            ResetForm();
            rescanDiscsButton_Click(sender, e);
        }

        private void DetectPhysicalDiscStatus()
        {
            settingsButton.Enabled = false;
            rescanDiscsButton.Enabled = false;
            step1Label.Text = "Checking inserted disc...";
            currentDumper = new(new());
            discBackgroundWorker.RunWorkerAsync(currentDumper);
        }

        private void DetectPs3DiscGame(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                dumper.DetectDisc("",
                    d =>
                                  {
                                      var items = new NameValueCollection
                                      {
                                          [Patterns.ProductCode] = d.ProductCode,
                                          [Patterns.ProductCodeLetters] = d.ProductCode?.Substring(0, 4),
                                          [Patterns.ProductCodeNumbers] = d.ProductCode?.Substring(4),
                                          [Patterns.Title] = d.Title,
                                          [Patterns.Region] = RegionMapping[d.ProductCode?.Substring(2, 1) ?? ""],
                                      };
                                      return PatternFormatter.Format(settings.DumpNameTemplate, items);
                                  });
            }
            catch { }
            doWorkEventArgs.Result = dumper;
        }

        private void DetectPs3DiscGameFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            discBackgroundWorker.DoWork -= DetectPs3DiscGame;
            discBackgroundWorker.RunWorkerCompleted -= DetectPs3DiscGameFinished;
            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(dumper.ProductCode))
            {
                ResetForm();
                return;
            }

            rescanDiscsButton.Visible = false;
            step1StatusLabel.Text = "✔";
            step1Label.Text = "PS3 game disc detected";
            step2StatusLabel.Text = "⏳";
            step2Label.Enabled = true;
            step2Label.Text = "Looking for the disc key...";

            productCodeLabel.Text = dumper.ProductCode;
            gameTitleLabel.Text = dumper.Title;
            discSizeLabel.Text = $"{dumper.TotalFileSize.AsStorageUnit()} ({dumper.TotalFileCount} files)";
            //irdMatchLabel.Text = string.IsNullOrEmpty(dumper.DiscKeyFilename) ? "❌" : "✔";

            discBackgroundWorker.DoWork += FindMatchingIrd;
            discBackgroundWorker.RunWorkerCompleted += FindMatchingIrdFinished;
            discBackgroundWorker.RunWorkerAsync(dumper);
        }

        private void FindMatchingIrd(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                dumper.FindDiscKeyAsync(settings.IrdDir).Wait(dumper.Cts.Token);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to find matching key");
//                MessageBox.Show(e.Message, "Disc check error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dumper.Cts.Cancel();
            }
            doWorkEventArgs.Result = dumper;
        }

        private void FindMatchingIrdFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            discBackgroundWorker.DoWork -= FindMatchingIrd;
            discBackgroundWorker.RunWorkerCompleted -= FindMatchingIrdFinished;
            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
            {
                cancelDiscDumpButton_Click(null, null);
                return;
            }

            settingsButton.Enabled = true;
            if (dumper.DiscKeyFilename == null)
            {
                irdMatchLabel.Text = "No match found";
                step2StatusLabel.Text = "❌";
                step2Label.Text = "Select a disc key file...";
                selectIrdButton.Enabled = true;
                selectIrdButton.Visible = true;
            }
            else
            {
                step2StatusLabel.Text = "✔";
                step2Label.Text = "Matched disc key selected";
                step3StatusLabel.Text = "▶";
                step3Label.Text = "Start disc decryption...";
                step3Label.Enabled = true;
                label3.Text = $"Matching {(dumper.DiscKeyType == KeyType.Ird ? "IRD" : "Key")}:";
                irdMatchLabel.Text = Path.GetFileNameWithoutExtension(dumper.DiscKeyFilename);
                rescanDiscsButton.Visible = false;
                rescanDiscsButton.Enabled = false;
                startDumpingButton.Enabled = true;
                startDumpingButton.Visible = true;
            }
        }

        private void DumpDisc(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var backgroundWorker = (BackgroundWorker)sender;
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                var threadCts = new CancellationTokenSource();
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(threadCts.Token, dumper.Cts.Token);
                var monitor = new Thread(() =>
                                         {
                                             try
                                             {
                                                 do
                                                 {
                                                     if (dumper.TotalSectors > 0 && backgroundWorker.IsBusy && !backgroundWorker.CancellationPending)
                                                        try { backgroundWorker.ReportProgress((int)(dumper.CurrentSector * 10000L / dumper.TotalSectors), dumper); } catch { }
                                                     Task.Delay(1000, combinedToken.Token).GetAwaiter().GetResult();
                                                 } while (!combinedToken.Token.IsCancellationRequested);
                                             }
                                             catch (TaskCanceledException)
                                             {
                                             }
                                         });
                monitor.Start();
                dumper.DumpAsync(settings.OutputDir).Wait(dumper.Cts.Token);
                threadCts.Cancel();
                monitor.Join(100);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to dump the disc");
                MessageBox.Show(e.Message, "Disc dumping error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dumper.Cts.Cancel();
            }
            doWorkEventArgs.Result = dumper;
        }

        private void DumpDiscFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            discBackgroundWorker.DoWork -= DumpDisc;
            discBackgroundWorker.RunWorkerCompleted -= DumpDiscFinished;
            discBackgroundWorker.ProgressChanged -= DiscDumpUpdateProgress;

            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
            {
                ResetForm();
                return;
            }

            settingsButton.Enabled = true;
            cancelDiscDumpButton.Visible = false;
            cancelDiscDumpButton.Enabled = false;
            dumpingProgressBar.Visible = false;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, Handle);
            TaskbarManager.Instance.SetProgressValue(0, dumpingProgressBar.Maximum, Handle);
            dumpingProgressLabel.Visible = false;
            dumpingProgressLabel.Text = "";
            step3StatusLabel.Text = "✔";
            step3Label.Text = "Files are decrypted and copied";
            step4Label.Enabled = true;
            rescanDiscsButton.Enabled = true;
            rescanDiscsButton.Visible = true;

            if (dumper.ValidationStatus == false)
            {
                step4StatusLabel.Text = "❌";
                step4Label.Text = "Dump is corrupted";
            }
            else if (dumper.ValidationStatus == true)
            {
                step4StatusLabel.Text = "✔";
                step4Label.Text = "Dump is valid";
            }
            else
            {
                step4StatusLabel.Text = "❔";
                step4Label.Text = "No validation info available";
            }
        }

        private void DiscDumpUpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            var dumper = (Dumper)e.UserState;
            dumpingProgressBar.Value = e.ProgressPercentage;
            dumpingProgressLabel.Text = $"{(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
            TaskbarManager.Instance.SetProgressValue(e.ProgressPercentage, dumpingProgressBar.Maximum, Handle);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            currentDumper?.Cts.Cancel();
        }
    }
}
