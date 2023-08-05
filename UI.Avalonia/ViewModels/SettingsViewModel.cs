using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

namespace UI.Avalonia.ViewModels;

public partial class SettingsViewModel: ViewModelBase
{
    public SettingsViewModel() => pageTitle = "Settings";

    private const string defaultPattern = $"%{Patterns.Title}% [%{Patterns.ProductCode}%]";
    
    [ObservableProperty] private string outputDir = Path.GetFullPath(".");
    [ObservableProperty] private string irdDir = Path.GetFullPath("ird");

    [ObservableProperty] private bool discInfoExpanded = true;
    [ObservableProperty] private bool configured;
    
    [ObservableProperty] private string templatePreview = FormatPreview(Path.GetFullPath("."), defaultPattern, testItems);
    [ObservableProperty] private string dumpNameTemplate = defaultPattern;
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

    public string DumperVersion => Dumper.Version;
    public string CurrentYear => DateTime.Now.Year.ToString();
    public static string ProjectUrl => "https://github.com/13xforever/ps3-disc-dumper";
    public static string SubmitIssueUrl => $"{ProjectUrl}/issues/new/choose";
    
    private readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());

    private static string FormatPreview(string outDir, string template, NameValueCollection items)
        => Path.Combine(
            Path.GetRelativePath(".", outDir),
            PatternFormatter.Format(template.Trim(), items)
        );

    partial void OnOutputDirChanged(string value)
        => TemplatePreview = FormatPreview(value, DumpNameTemplate, TestItems);
    
    partial void OnDumpNameTemplateChanged(string value)
        => TemplatePreview = FormatPreview(OutputDir, value, TestItems);

    partial void OnTestItemsChanged(NameValueCollection value)
        => TemplatePreview = FormatPreview(OutputDir, DumpNameTemplate, value);

    [RelayCommand]
    private void ResetTemplate() => DumpNameTemplate = defaultPattern;

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
}