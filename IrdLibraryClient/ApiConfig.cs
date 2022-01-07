using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace IrdLibraryClient
{
    public static class ApiConfig
    {
        public static readonly CancellationTokenSource Cts = new();
        public static readonly string IrdCachePath = "./ird/";

        static ApiConfig()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                IrdCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) ,"ps3-iso-dumper/ird/");
        }
    }
}