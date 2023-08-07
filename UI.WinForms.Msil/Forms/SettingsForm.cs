using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

namespace UI.WinForms.Msil;

public partial class SettingsForm : Form
{
    private readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
    private Color defaultTextBoxBackground;

    private static readonly NameValueCollection TestItems = new()
    {
        [Patterns.ProductCode] = "BLUS12345",
        [Patterns.ProductCodeLetters] = "BLUS",
        [Patterns.ProductCodeNumbers] = "12345",
        [Patterns.Title] = "My PS3 Game Can't Be This Cute",
        [Patterns.Region] = "US",
    };

    public SettingsForm()
    {
        InitializeComponent();
        defaultTextBoxBackground = outputTextBox.BackColor;
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        var settings = SettingsProvider.Settings;
        outputTextBox.Text = settings.OutputDir;
        irdTextBox.Text = settings.IrdDir;
        namePatternTextBox.Text = settings.DumpNameTemplate;
        namePatternTextBox_TextChanged();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void saveButton_Click(object sender, EventArgs e)
    {
        var settings = SettingsProvider.Settings;
        settings.OutputDir = outputTextBox.Text;
        settings.IrdDir = irdTextBox.Text;
        settings.DumpNameTemplate = namePatternTextBox.Text.Trim();
        SettingsProvider.Settings = settings;
        SettingsProvider.Save();
        Close();
    }

    private void namePatternTextBox_TextChanged(object sender = null, EventArgs e = null)
    {
        namePatternExampleLabel.Text = PatternFormatter.Format(namePatternTextBox.Text.Trim(), TestItems);
        outputTextBox_TextChanged(sender, e);
    }

    private void outputBrowseButton_Click(object sender, EventArgs e)
    {
        var fullPath = Path.GetFullPath(outputTextBox.Text);
        var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder",
            SelectedPath = fullPath,
        };
        var dialogResult = dialog.ShowDialog();
        if (dialogResult != DialogResult.OK || string.IsNullOrEmpty(dialog.SelectedPath) || !Directory.Exists(dialog.SelectedPath))
            return;

        outputTextBox.Text = dialog.SelectedPath;
    }

    private void irdButton_Click(object sender, EventArgs e)
    {
        var fullPath = Path.GetFullPath(irdTextBox.Text);
        var dialog = new FolderBrowserDialog
        {
            Description = "Select IRD cache folder",
            SelectedPath = fullPath,
        };
        var dialogResult = dialog.ShowDialog();
        if (dialogResult != DialogResult.OK || string.IsNullOrEmpty(dialog.SelectedPath) || !Directory.Exists(dialog.SelectedPath))
            return;

        irdTextBox.Text = dialog.SelectedPath;
    }

    private void outputTextBox_TextChanged(object sender, EventArgs e)
    {
        var isOutputValid = true;
        if (outputTextBox.Text is null or "")
            isOutputValid = false;
        else if (Path.GetPathRoot(Path.GetFullPath(outputTextBox.Text)) is not string driveLetter || !Directory.Exists(driveLetter))
            isOutputValid = false;
        if (!isOutputValid)
            outputTextBox.BackColor = Color.DarkRed;
        else
            outputTextBox.BackColor = defaultTextBoxBackground;

        var isIrdValid = true;
        if (irdTextBox.Text is null or "")
            isIrdValid = false;
        else if (Path.GetPathRoot(Path.GetFullPath(irdTextBox.Text)) is not string driveLetter || !Directory.Exists(driveLetter))
            isIrdValid = false;
        if (!isIrdValid)
            irdTextBox.BackColor = Color.DarkRed;
        else
            irdTextBox.BackColor = defaultTextBoxBackground;

        var isPatternValid = true;
        if (namePatternTextBox.Text is null or "")
            isPatternValid = false;
        else
        {
            var testData1 = new NameValueCollection
            {
                [Patterns.Title] = "Title",
                [Patterns.ProductCode] = "BLUS98765",
                [Patterns.ProductCodeLetters] = "BLUS",
                [Patterns.ProductCodeNumbers] = "98765",
                [Patterns.Region] = "US",
            };
            var testData2 = new NameValueCollection
            {
                [Patterns.Title] = "タイトル",
                [Patterns.ProductCode] = "BLJM01234",
                [Patterns.ProductCodeLetters] = "BLJM",
                [Patterns.ProductCodeNumbers] = "01234",
                [Patterns.Region] = "JP",
            };
            var name1 = PatternFormatter.Format(namePatternTextBox.Text, testData1);
            var name2 = PatternFormatter.Format(namePatternTextBox.Text, testData2);
            if (name1 is null or "" || name2 is null or "" || name1 == name2 || name1.Any(InvalidFileNameChars.Contains))
                isPatternValid = false;
        }
        if (!isPatternValid)
            namePatternTextBox.BackColor = Color.DarkRed;
        else
            namePatternTextBox.BackColor = defaultTextBoxBackground;

        saveButton.Enabled = isOutputValid && isIrdValid && isPatternValid;
    }

    private void defaultsButton_Click(object sender, EventArgs e)
    {
        var defaultSettings = new Settings();
        outputTextBox.Text = defaultSettings.OutputDir;
        irdTextBox.Text = defaultSettings.IrdDir;
        namePatternTextBox.Text = defaultSettings.DumpNameTemplate;
        namePatternTextBox_TextChanged();
    }
}