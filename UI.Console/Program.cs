using System;
using System.Threading.Tasks;
using IrdLibraryClient;
using Ps3DiscDumper;

namespace UIConsole
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "PS3 Disc Dumper";
            if (args.Length > 1)
            {
                Console.WriteLine("Expected one arguments: output folder");
                return;
            }

            var output = args.Length == 1 ? args[0] : @".\";
            var dumper = new Dumper(output, ApiConfig.Cts.Token);
            await dumper.DetectDiscAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(dumper.OutputDir))
            {
                ApiConfig.Log.Info("No compatible disc was found, exiting");
                return;
            }

            await dumper.DumpAsync().ConfigureAwait(false);
        }
    }
}
