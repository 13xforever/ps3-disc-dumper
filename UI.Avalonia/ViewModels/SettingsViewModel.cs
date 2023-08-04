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
    [ObservableProperty] private string dumpNameTemplate = defaultPattern;
    [ObservableProperty] private bool discInfoExpanded = true;
    [ObservableProperty] private bool configured;

    [ObservableProperty] private string templatePreview = FormatPreview(Path.GetFullPath("."), defaultPattern);
    
    private readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());

    private static readonly NameValueCollection TestItems = new()
    {
        [Patterns.ProductCode] = "BLUS12345",
        [Patterns.ProductCodeLetters] = "BLUS",
        [Patterns.ProductCodeNumbers] = "12345",
        [Patterns.Title] = "My PS3 Game Can't Be This Cute",
        [Patterns.Region] = "US",
    };


    private static string FormatPreview(string outDir, string value)
        => Path.Combine(
            Path.GetRelativePath(".", outDir),
            PatternFormatter.Format(value.Trim(), TestItems)
        );
    
    partial void OnDumpNameTemplateChanged(string value)
        => TemplatePreview = FormatPreview(OutputDir, value);
}