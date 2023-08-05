using System;
using System.IO;
using System.Text.Json;
using IrdLibraryClient;
using SpecialFolder = System.Environment.SpecialFolder;

namespace Ps3DiscDumper;

public static class SettingsProvider
{
    private static readonly string settingsFolder;
    private static readonly string settingsPath;
    private static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true, };
    
    static SettingsProvider()
    {
        try
        {
            Log.Info("Loading settings…");
            settingsFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "ps3-disc-dumper");
            settingsPath = Path.Combine(settingsFolder, "settings.json");
            if (!File.Exists(settingsPath))
                return;
            
            using var file = File.Open(settingsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            var settingsContent = reader.ReadToEnd();
            Log.Info($"Current settings: {settingsContent}");
            savedSettings = JsonSerializer.Deserialize<Settings>(settingsContent);
            Settings = savedSettings;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to initialize settings");
        }
    }

    public static void Save()
    {
        var tmp = Settings;
        if (tmp.Equals(savedSettings))
            return;

        try
        {
            if (!Directory.Exists(settingsFolder))
                Directory.CreateDirectory(settingsFolder);

            using var file = File.Open(settingsPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            JsonSerializer.Serialize(file, tmp, serializerOptions);
            file.Flush();
            savedSettings = tmp;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to save settings");
        }
    }

    private static Settings savedSettings = new();
    public static Settings Settings { get; set; } = new();
}