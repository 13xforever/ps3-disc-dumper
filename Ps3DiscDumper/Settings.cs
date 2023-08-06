using System;
using System.Text.Json.Serialization;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper;

public struct Settings
{
    public Settings() { }

    [JsonIgnore]
    public const string DefaultPattern = $"%{Patterns.Title}% [%{Patterns.ProductCode}%]";

    public string OutputDir { get; set; } = ".";
    public string IrdDir { get; set; } = "ird";
    public string DumpNameTemplate { get; set; } = DefaultPattern;
    public bool ShowDetails { get; set; } = true;
    public bool EnableTransparency { get; set; } = true;

    private static StringComparison Comparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    private static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public override bool Equals(object obj)
        => obj is Settings other && Equals(other);

    public bool Equals(Settings other)
        => string.Equals(OutputDir, other.OutputDir, Comparison)
           && string.Equals(IrdDir, other.IrdDir, Comparison)
           && string.Equals(DumpNameTemplate, other.DumpNameTemplate, Comparison)
           && EnableTransparency == other.EnableTransparency
           && ShowDetails == other.ShowDetails;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(OutputDir, Comparer);
        hashCode.Add(IrdDir, Comparer);
        hashCode.Add(DumpNameTemplate, Comparer);
        hashCode.Add(EnableTransparency);
        hashCode.Add(ShowDetails);
        return hashCode.ToHashCode();
    }
}