namespace Ps3DiscDumper.Utils
{
    public static class StorageUnits
    {
        private const long UnderKB = 1000;
        private const long UnderMB = 1000 * 1024;
        private const long UnderGB = 1000 * 1024 * 1024;

        public static string AsStorageUnit(this long bytes)
        {
            if (bytes < UnderKB)
                return $"{bytes} byte{(bytes == 1 ? "" : "s")}";
            if (bytes < UnderMB)
                return $"{bytes / 1024.0:0.##} KB";
            if (bytes < UnderGB)
                return $"{bytes / 1024.0 / 1024:0.##} MB";
            return $"{bytes / 1024.0 / 1024 / 1024:0.##} GB";
        }
    }
}
