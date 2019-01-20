using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IrdLibraryClient.IrdFormat;
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

        private static readonly NameValueCollection RegionMapping = new NameValueCollection
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
            settings.Reload();
            if (!settings.Configured)
            {
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                settings.Reload();
            }

            ResetForm();
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
            step2Label.Text = "Select matching IRD file";
            step3Label.Text = "Decrypt and copy files";
            step4Label.Text = "Validate integrity";

            step2Label.Enabled = false;
            step3Label.Enabled = false;
            step4Label.Enabled = false;

            currentDumper = null;
            discBackgroundWorker?.Dispose();
            discBackgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            discBackgroundWorker.DoWork += DetectPs3DiscGame;
            discBackgroundWorker.RunWorkerCompleted += DetectPs3DiscGameFinished;

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
                Filter = "IRD file (*.ird)|*.ird|All files|*",
                Title = "Select an IRD file",
                SupportMultiDottedExtensions = true,
                InitialDirectory = settings.IrdDir,
            };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != DialogResult.OK || string.IsNullOrEmpty(dialog.FileName) || !File.Exists(dialog.FileName))
                return;

            var irdPath = dialog.FileName;
            try
            {
                var ird = IrdParser.Parse(File.ReadAllBytes(irdPath));
                var irdFilename = Path.GetFileName(irdPath);
                var cacheFilename = Path.Combine(settings.IrdDir, irdFilename);
                if (!File.Exists(cacheFilename))
                    File.Copy(irdPath, cacheFilename);

                if (!currentDumper.IsFullMatch(ird))
                {
                    if (currentDumper.IsFilenameSetMatch(ird))
                    {
                        var msgResult = MessageBox.Show(
                            "Selected IRD file does not fully match with the selected PS3 game disc.\n" +
                            "Successful decryption cannot be guaranteed.\n" +
                            "Do you want to use this IRD file anyway?",
                            "IRD file check",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        if (msgResult == DialogResult.No)
                            return;
                    }
                    else
                    {
                        MessageBox.Show("Selected IRD file contains incompatible file set, and cannot be used with the selected PS3 game disc.", "IRD file check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                currentDumper.Ird = ird;
                currentDumper.IrdFilename = irdFilename;
                selectIrdButton.Visible = false;
                selectIrdButton.Enabled = false;
                FindMatchingIrdFinished(sender, new RunWorkerCompletedEventArgs(currentDumper, null, currentDumper?.Cts.IsCancellationRequested ?? true));
            }
            catch (Exception ex)
            {
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

            startDumpingButton.Enabled = false;
            startDumpingButton.Visible = false;

            discBackgroundWorker.DoWork += DumpDisc;
            discBackgroundWorker.RunWorkerCompleted += DumpDiscFinished;
            discBackgroundWorker.ProgressChanged += DiscDumpUpdateProgress;
            discBackgroundWorker.RunWorkerAsync(currentDumper);

            cancelDiscDumpButton.Enabled = true;
            cancelDiscDumpButton.Visible = true;
            dumpingProgressBar.Visible = true;
            dumpingProgressLabel.Visible = true;
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
            rescanDiscsButton.Enabled = false;
            step1Label.Text = "Checking inserted disc...";
            currentDumper = new Dumper(new CancellationTokenSource());
            discBackgroundWorker.RunWorkerAsync(currentDumper);
        }

        private void DetectPs3DiscGame(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var dumper = (Dumper)doWorkEventArgs.Argument;
            try
            {
                dumper.DetectDisc(d =>
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

            if (dumper.Ird == null)
            {
                irdMatchLabel.Text = "No match found";
                step2StatusLabel.Text = "❌";
                step2Label.Text = "Select matching IRD file...";
                selectIrdButton.Enabled = true;
                selectIrdButton.Visible = true;
            }
            else
            {
                step2StatusLabel.Text = "✔";
                step2Label.Text = "Matched IRD file selected";
                step3StatusLabel.Text = "▶";
                step3Label.Text = "Start disc decryption...";
                step3Label.Enabled = true;
                irdMatchLabel.Text = Path.GetFileNameWithoutExtension(dumper.IrdFilename);
                rescanDiscsButton.Visible = false;
                rescanDiscsButton.Enabled = false;
                startDumpingButton.Enabled = true;
                startDumpingButton.Visible = true;
                discBackgroundWorker.DoWork -= FindMatchingIrd;
                discBackgroundWorker.RunWorkerCompleted -= FindMatchingIrdFinished;
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
                                                     if (dumper.CurrentSector > 0)
                                                         backgroundWorker.ReportProgress((int)(dumper.CurrentSector * 10000L / dumper.TotalSectors), dumper);
                                                     Task.Delay(1000, combinedToken.Token).GetAwaiter().GetResult();
                                                 } while (!combinedToken.Token.IsCancellationRequested);
                                             }
                                             catch (TaskCanceledException)
                                             {
                                             }
                                         });
                monitor.Start();
                dumper.DumpAsync(settings.OutputDir).Wait(dumper.Cts.Token);
                monitor.Join(100);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Disc dumping error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            doWorkEventArgs.Result = dumper;
        }

        private void DumpDiscFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            var dumper = (Dumper)e.Result;
            if (e.Cancelled || dumper.Cts.IsCancellationRequested)
                return;

            cancelDiscDumpButton.Visible = false;
            cancelDiscDumpButton.Enabled = false;
            dumpingProgressBar.Visible = false;
            step3StatusLabel.Text = "✔";
            step3Label.Text = "Files are decrypted and copied";
            step4Label.Enabled = true;

            if (dumper.BrokenFiles.Any())
            {
                step4StatusLabel.Text = "❌";
                step4Label.Text = "Dump is corrupted";
            }
            else
            {
                step4StatusLabel.Text = "✔";
                step4Label.Text = "Dump is valid";
            }
            discBackgroundWorker.DoWork -= DumpDisc;
            discBackgroundWorker.RunWorkerCompleted -= DumpDiscFinished;
            discBackgroundWorker.ProgressChanged -= DiscDumpUpdateProgress;
        }

        private void DiscDumpUpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            var dumper = (Dumper)e.UserState;
            dumpingProgressBar.Value = e.ProgressPercentage;
            dumpingProgressLabel.Text = $"File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
        }
    }
}
