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
            if (args.Length != 2)
            {
                Console.WriteLine("Expected two arguments: input and output folders");
                return;
            }

            var dumper = new Dumper(args[0], args[1], ApiConfig.Cts.Token);
            await dumper.DetectDiscAsync().ConfigureAwait(false);
            if (!string.IsNullOrEmpty(dumper.OutputDir))
            {
                ApiConfig.Log.Info("Dumping disc from " + args[0]);
            }
            await dumper.DumpAsync().ConfigureAwait(false);
        }
    }
}
