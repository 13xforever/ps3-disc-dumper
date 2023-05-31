namespace Ps3DiscDumper.Utils;

public static class StorageUnits
{
    private const long UnderKB = 1000L;
    private const long UnderMB = 1000L * 1024;
    private const long UnderGB = 1000L * 1024 * 1024;
    private const long UnderTB = 1000L * 1024 * 1024 * 1024;

    public static string AsStorageUnit(this long bytes)
    {
        if (bytes < UnderKB)
            return $"{bytes} byte{(bytes == 1 ? "" : "s")}";
        if (bytes < UnderMB)
            return $"{bytes / 1024.0:0.##} KB";
        if (bytes < UnderGB)
            return $"{bytes / 1024.0 / 1024:0.##} MB";
        if (bytes < UnderTB)
            return $"{bytes / 1024.0 / 1024 / 1024:0.##} GB";
        return $"{bytes / 1024.0 / 1024 / 1024:0.##} TB";
    }
}