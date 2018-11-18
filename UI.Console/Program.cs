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

            await new Dumper().DumpAsync(args[0], args[1], ApiConfig.Cts.Token);
        }
    }
}
