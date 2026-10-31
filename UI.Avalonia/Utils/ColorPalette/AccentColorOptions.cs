namespace UI.Avalonia.Utils.ColorPalette;

public sealed record AccentColorOptions
{
    public bool AutoColorAccent { get; init; }
    public bool UseNewAutoColorAccentAlgorithm { get; init; } = true;
    public int MinSaturation { get; init; } = 150;
    public static AccentColorOptions Default { get; } = new();
}