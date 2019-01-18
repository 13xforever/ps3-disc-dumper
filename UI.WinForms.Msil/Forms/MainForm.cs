using System;
using System.Windows.Forms;

namespace UI.WinForms.Msil
{
    public partial class MainForm : Form
    {
        private readonly Settings Settings = new Settings();

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
        }
    }
}
