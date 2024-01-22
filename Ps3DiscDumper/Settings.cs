using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Ps3DiscDumper.Utils;

using SpecialFolder = System.Environment.SpecialFolder;

namespace Ps3DiscDumper;

public struct Settings
{
    public Settings() { }

    [JsonIgnore] public const string DefaultPattern = $"%{Patterns.Title}% [%{Patterns.ProductCode}%]";
    [JsonIgnore] public static readonly string[] BdmvFolders = ["BDMV", "AACS", "CERTIFICATE"];
    [JsonIgnore] public static readonly string[] Ps3UpdateFolder = ["PS3_UPDATE"];

    public string OutputDir { get; set; } = ".";
    public string IrdDir { get; set; } = Path.Combine(Environment.GetFolderPath(SpecialFolder.ApplicationData) ,"ps3-iso-dumper", "ird");
    public string DumpNameTemplate { get; set; } = DefaultPattern;
    public bool ShowDetails { get; set; } = true;
    public bool EnableTransparency { get; set; } = true;
    public bool PreferSystemAccent { get; set; } = !OperatingSystem.IsWindows();
    public bool StayOnTop { get; set; } = false;
    public bool CopyBdmv { get; set; } = false;
    public bool CopyPs3Update { get; set; } = false;

    [JsonIgnore] public bool FilterRequired => !CopyBdmv || !CopyPs3Update;
    [JsonIgnore] public string[] FilterDirList => (CopyBdmv, CopyPs3Update) switch
    {
        (false, false) => BdmvFolders.Concat(Ps3UpdateFolder).ToArray(),
        (false, true) => BdmvFolders,
        (true, false) => Ps3UpdateFolder,
        _ => Array.Empty<string>(),
    };

    private static StringComparison Comparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    private static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public override bool Equals(object obj)
        => obj is Settings other && Equals(other);

    public bool Equals(Settings other)
        => string.Equals(OutputDir, other.OutputDir, Comparison)
           && string.Equals(IrdDir, other.IrdDir, Comparison)
           && string.Equals(DumpNameTemplate, other.DumpNameTemplate, Comparison)
           && ShowDetails == other.ShowDetails
           && EnableTransparency == other.EnableTransparency
           && PreferSystemAccent == other.PreferSystemAccent
           && StayOnTop == other.StayOnTop
           && CopyBdmv == other.CopyBdmv
           && CopyPs3Update == other.CopyPs3Update;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(OutputDir, Comparer);
        hashCode.Add(IrdDir, Comparer);
        hashCode.Add(DumpNameTemplate, Comparer);
        hashCode.Add(ShowDetails);
        hashCode.Add(EnableTransparency);
        hashCode.Add(PreferSystemAccent);
        hashCode.Add(StayOnTop);
        hashCode.Add(CopyBdmv);
        hashCode.Add(CopyPs3Update);
        return hashCode.ToHashCode();
    }
}