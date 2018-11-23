namespace Ps3DiscDumper.Utils
{
    public static class StorageUnits
    {
        private const long UnderKiB = 1024;
        private const long UnderMiB = 1024 * 1024;
        private const long UnderGiB = 1024 * 1024 * 1024;

        public static string AsStorageUnit(this long bytes)
        {
            if (bytes < UnderKiB)
                return $"{bytes} byte{(bytes == 1 ? "" : "s")}";
            if (bytes < UnderMiB)
                return $"{bytes / 1024.0:0.##} KiB";
            if (bytes < UnderGiB)
                return $"{bytes / 1024.0 / 1024:0.##} MiB";
            return $"{bytes / 1024.0 / 1024 / 1024:0.##} GiB";
        }
    }
}
