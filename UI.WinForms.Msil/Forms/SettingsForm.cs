using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace UI.WinForms.Msil
{
    public partial class SettingsForm : Form
    {
        private readonly Settings Settings = new Settings();
        private static readonly NameValueCollection TestItems = new NameValueCollection
        {
            [Patterns.ProductCode] = "BLUS12345",
            [Patterns.ProductCodeLetters] = "BLUS",
            [Patterns.ProductCodeNumbers] = "12345",
            [Patterns.Title] = "Weebs in Space",
            [Patterns.Region] = "US",
        };

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            Settings.Reload();
            outputTextBox.Text = Settings.OutputDir;
            irdTextBox.Text = Settings.IrdDir;
            namePatternTextBox.Text = Settings.DumpNameTemplate;
            namePatternTextBox_TextChanged();
/*
            if (!Settings.Configured)
            {
                Settings.Configured = true;
                Settings.Save();
            }
*/
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.OutputDir = outputTextBox.Text;
            Settings.IrdDir = irdTextBox.Text;
            Settings.DumpNameTemplate = namePatternTextBox.Text.Trim();
            Settings.Configured = true;
            Settings.Save();
        }

        private void namePatternTextBox_TextChanged(object sender = null, EventArgs e = null)
        {
            namePatternExampleLabel.Text = PatternFormatter.Format(namePatternTextBox.Text.Trim(), TestItems);
        }
    }
}
