using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using IrdLibraryClient;
using SpecialFolder = System.Environment.SpecialFolder;

namespace Ps3DiscDumper;

public static partial class SettingsProvider
{
    private static readonly string SettingsFolder;
    private static readonly string SettingsPath;
    
    static SettingsProvider()
    {
        Log.Info("Loading settings…");
        SettingsFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "ps3-disc-dumper");
        SettingsPath = Path.Combine(SettingsFolder, "settings.json");
        try
        {
            if (File.Exists(SettingsPath))
            {
                Log.Debug("Found existing settings file, reading…");
                savedSettings = Read() ?? savedSettings;
            }
            else
            {
                if (OperatingSystem.IsWindows())
                {
                    Log.Debug("No existing settings file was found, trying to import legacy settings…");
                    savedSettings = Import() ?? savedSettings;
                }
                else
                {
                    Log.Debug("Using default settings");
                }
            }
            Settings = savedSettings;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to initialize settings");
        }
    }

    private static Settings? Read()
    {
        try
        {
            using var file = File.Open(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            var settingsContent = reader.ReadToEnd();
            Log.Info($"Current settings: {settingsContent}");
            return JsonSerializer.Deserialize(settingsContent, SettingsSerializer.Default.Settings);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to initialize settings");
            return null;
        }
    }

    private static Settings? Import()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return null;

            var localAppDataPath = Environment.GetFolderPath(SpecialFolder.LocalApplicationData);
            var oldRootDir = Path.Combine(localAppDataPath, "UI");
            if (!Directory.Exists(oldRootDir))
                return null;

            var lastUsedConfigPath = Directory
                .GetDirectories(oldRootDir, "ps3-disc-dumper*", SearchOption.TopDirectoryOnly)
                .Select(d => Path.Combine(d,"1.0.0.0", "user.config"))
                .Where(File.Exists)
                .MaxBy(f => new FileInfo(f).LastWriteTime);
            if (lastUsedConfigPath is null)
                return null;
            
            Log.Info($@"Importing old config from %LocalAppData%\{Path.GetRelativePath(localAppDataPath, lastUsedConfigPath)}");
            var xml = XDocument.Load(lastUsedConfigPath);
            if (xml.Root?.Element("userSettings")?.FirstNode is not XElement settingsNode)
                return null;

            var template = (string?)settingsNode.Descendants("setting")
                .FirstOrDefault(el => (string?)el.Attribute("name") == "DumpNameTemplate")?
                .Element("value");
            var output = (string?)settingsNode.Descendants("setting")
                .FirstOrDefault(el => (string?)el.Attribute("name") == "OutputDir")?
                .Element("value");
            var ird = (string?)settingsNode.Descendants("setting")
                .FirstOrDefault(el => (string?)el.Attribute("name") == "IrdDir")?
                .Element("value");
            var result = new Settings();
            if (template is { Length: >0 })
                result = result with { DumpNameTemplate = template };
            if (output is { Length: >0 } && Directory.Exists(output))
                result = result with { OutputDir = output };
            if (ird is { Length: >0 } && Directory.Exists(ird))
                result = result with { IrdDir = ird };
            var newSettingsContent = JsonSerializer.Serialize(result, SettingsSerializer.Default.Settings);
            Log.Info($"Imported settings: {newSettingsContent}");
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to import old settings file");
            return null;
        }
    }
    
    public static void Save()
    {
        var tmp = Settings;
        if (tmp.Equals(savedSettings))
            return;

        try
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            var settingsContent = JsonSerializer.Serialize(tmp, SettingsSerializer.Default.Settings);
            Log.Info($"Updated settings: {settingsContent}");
            using var file = File.Open(SettingsPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(file);
            writer.Write(settingsContent);
            writer.Flush();
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
    
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Settings))]
    internal partial class SettingsSerializer: JsonSerializerContext;
}
