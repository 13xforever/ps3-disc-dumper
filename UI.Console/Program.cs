using System;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using Ps3DiscDumper;

namespace UIConsole
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Log.Info("PS3 Disc Dumper v1.1.2");
            var lastDiscId = "";
start:
            const string titleBase = "PS3 Disc Dumper";
            var title = titleBase;
            Console.Title = title;
            try
            {
                if (args.Length > 1)
                {
                    Console.WriteLine("Expected one arguments: output folder");
                    return;
                }

                var output = args.Length == 1 ? args[0] : ".";
                var dumper = new Dumper();
                dumper.DetectDisc();
                await dumper.FindIrdAsync(output, ApiConfig.IrdCachePath, ApiConfig.Cts.Token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(dumper.OutputDir))
                {
                    Log.Info("No compatible disc was found, exiting");
                    return;
                }
                if (lastDiscId == dumper.ProductCode)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("You're dumping the same disc, are you sure you want to continue? (Y/N, default is N)");
                    Console.ResetColor();
                    var confirmKey = Console.ReadKey(true);
                    switch (confirmKey.Key)
                    {
                        case ConsoleKey.Y:
                            break;
                        default:
                            throw new OperationCanceledException("Aborting re-dump of the same disc");
                    }
                }
                lastDiscId = dumper.ProductCode;

                title += " - " + dumper.Title;
                var monitor = new Thread(() =>
                {
                    try
                    {
                        do
                        {
                            if (dumper.CurrentSector > 0)
                                Console.Title = $"{title} - File {dumper.CurrentFileNumber} of {dumper.TotalFileCount} - {dumper.CurrentSector * 100.0 / dumper.TotalSectors:0.00}%";
                            Task.Delay(1000, ApiConfig.Cts.Token).GetAwaiter().GetResult();
                        } while (!ApiConfig.Cts.Token.IsCancellationRequested);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    Console.Title = title;
                });
                monitor.Start();

                dumper.Dump(output, ApiConfig.Cts.Token);

                ApiConfig.Cts.Cancel(false);
                monitor.Join(100);

                if (dumper.BrokenFiles.Count > 0)
                {
                    Log.Fatal("Dump is not valid");
                    foreach (var file in dumper.BrokenFiles)
                        Log.Error($"{file.error}: {file.filename}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Dump is valid");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
            Console.WriteLine("Press X or Ctrl-C to exit, any other key to start again...");
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.X:
                    return;
                default:
                    goto start;
            }
        }
    }
}
