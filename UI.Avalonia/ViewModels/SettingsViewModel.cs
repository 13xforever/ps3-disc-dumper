using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Ps3DiscDumper.Utils;

namespace UI.Avalonia.ViewModels;

public partial class SettingsViewModel: ViewModelBase
{
    public SettingsViewModel()
    {
        pageTitle = "Settings";
    }

    private const string defaultPattern = $"%{Patterns.Title}% [%{Patterns.ProductCode}%]";
    
    [ObservableProperty] private string outputDir = Path.GetFullPath(".");
    [ObservableProperty] private string irdDir = Path.GetFullPath("ird");

    [ObservableProperty] private bool discInfoExpanded = true;
    [ObservableProperty] private bool configured;
    
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
    
    [ObservableProperty] private string templatePreview = FormatPreview(Path.GetFullPath("."), defaultPattern, testItems);
    
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
   
}