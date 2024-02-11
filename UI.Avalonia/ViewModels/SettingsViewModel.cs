using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

using SpecialFolder = System.Environment.SpecialFolder;

namespace UI.Avalonia.ViewModels;

public partial class SettingsViewModel: ViewModelBase
{
    public SettingsViewModel() => pageTitle = "Settings";

    [NotifyPropertyChangedFor(nameof(OutputDirPreview))]
    [ObservableProperty] private string outputDir = SettingsProvider.Settings.OutputDir;
    [NotifyPropertyChangedFor(nameof(IrdDirPreview))]
    [ObservableProperty] private string irdDir = SettingsProvider.Settings.IrdDir;

    public string OutputDirPreview => FormatPathPreview(OutputDir);
    public string IrdDirPreview => FormatPathPreview(IrdDir);
    
    [ObservableProperty] private string templatePreview = FormatPreview(SettingsProvider.Settings.OutputDir, SettingsProvider.Settings.DumpNameTemplate, testItems);
    [ObservableProperty] private string dumpNameTemplate = SettingsProvider.Settings.DumpNameTemplate;
    [NotifyPropertyChangedFor(nameof(TestProductCode))]
    [NotifyPropertyChangedFor(nameof(TestProductCodeLetters))]
    [NotifyPropertyChangedFor(nameof(TestProductCodeNumbers))]
    [NotifyPropertyChangedFor(nameof(TestTitle))]
    [NotifyPropertyChangedFor(nameof(TestRegion))]
    [ObservableProperty]
    private static NameValueCollection testItems = new()
    {
        [Patterns.ProductCode] = "BLES02127",
        [Patterns.ProductCodeLetters] = "BLES",
        [Patterns.ProductCodeNumbers] = "02127",
        [Patterns.Title] = "Under Night In-Birth Exe:Late",
        [Patterns.Region] = "EU",
    };
    public string TestProductCode => testItems[Patterns.ProductCode]!;
    public string TestProductCodeLetters => testItems[Patterns.ProductCodeLetters]!;
    public string TestProductCodeNumbers => testItems[Patterns.ProductCodeNumbers]!;
    public string TestTitle => testItems[Patterns.Title]!;
    public string TestRegion => testItems[Patterns.Region]!;

    public bool LogAvailable => File.Exists(Log.LogPath); 
    public string LogPath => Log.LogPath;
    public string LogPathPreview => FormatPathPreview(Log.LogPath);
    public string DumperVersion => Dumper.Version;
    public string CurrentYear => DateTime.Now.Year.ToString();
    public static string ProjectUrl => "https://github.com/13xforever/ps3-disc-dumper";
    public static string SubmitIssueUrl => $"{ProjectUrl}/issues/new/choose";
    public static string WikiUrlBase => $"{ProjectUrl}/wiki/";
    public static string HybridDiscWikiLink => $"{WikiUrlBase}Hybrid-discs";

    private static string FormatPathPreview(string path)
    {
        if (path is "<n/a>")
            return "Couldn't create file due to permission or other issues";

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var homePath = Environment.GetFolderPath(SpecialFolder.UserProfile);
            if (path.StartsWith(homePath))
                return Path.Combine("~", Path.GetRelativePath(homePath, path));
        }

        if (path is "."
            || path.StartsWith("./")
            || OperatingSystem.IsWindows() && path.StartsWith(@".\"))
            return "<current folder>" + path[1..];
        
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine("<current folder>", path);
    }

    private static string FormatPreview(string outDir, string template, NameValueCollection items)
        => PatternFormatter.Format(template.Trim(), items);

    partial void OnOutputDirChanged(string value)
    {
        TemplatePreview = FormatPreview(value, DumpNameTemplate, TestItems);
        SettingsProvider.Settings = SettingsProvider.Settings with { OutputDir = value};
    }

    partial void OnIrdDirChanged(string value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { IrdDir = value };
    }

    partial void OnDumpNameTemplateChanged(string value)
    {
        TemplatePreview = FormatPreview(OutputDir, value, TestItems);
        SettingsProvider.Settings = SettingsProvider.Settings with { DumpNameTemplate = value };
    }

    partial void OnTestItemsChanged(NameValueCollection value)
        => TemplatePreview = FormatPreview(OutputDir, DumpNameTemplate, value);

    [RelayCommand]
    private void ResetTemplate() => DumpNameTemplate = Settings.DefaultPattern;

    [RelayCommand]
    private async Task SelectFolderAsync(string purpose)
    {
        var curSelectedPath = purpose switch
        {
            "output" => OutputDir,
            "ird" => IrdDir,
            _ => throw new NotImplementedException()
        };
        var itemsType = purpose switch
        {
            "output" => "disc dumps",
            "ird" => "cached disc keys",
            _ => throw new NotImplementedException()
        };
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
            {
                MainWindow.StorageProvider: { } sp
            })
            return;
        
        var curDir = await sp.TryGetFolderFromPathAsync(curSelectedPath).ConfigureAwait(false);
        var result = await sp.OpenFolderPickerAsync(new()
        {
            SuggestedStartLocation = curDir,
            Title = "Please select a folder to save " + itemsType,
        });
        if (result is [var newDir])
        {
            var newPath = newDir.Path.IsFile
                ? newDir.Path.LocalPath
                : newDir.Path.ToString();
            if (purpose is "output")
                OutputDir = newPath;
            else if (purpose is "ird")
                IrdDir = newPath;
        }
    }

    [RelayCommand]
    private void OpenFolder(string value)
    {
        var folder = value;
        if (File.Exists(folder))
            folder = Path.GetDirectoryName(value);
        folder = $"\"{folder}\"";
        ProcessStartInfo psi = OperatingSystem.IsWindows()
            ? new() { Verb = "open", FileName = folder, UseShellExecute = true, }
            : OperatingSystem.IsLinux()
                ? new() { FileName = "xdg-open", Arguments = folder, }
                : new() { FileName = "open", Arguments = folder, };
        try
        {
            Process.Start(psi);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to open log folder");
        }
    }
}