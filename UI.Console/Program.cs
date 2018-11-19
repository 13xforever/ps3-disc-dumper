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
            var title = "PS3 Disc Dumper";
            Console.Title = title;
            try
            {
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

                await dumper.DumpAsync().ConfigureAwait(false);

                ApiConfig.Cts.Cancel(false);
                monitor.Join(100);

                if (dumper.BrokenFiles.Count > 0)
                {
                    ApiConfig.Log.Fatal("Dump is not valid");
                    foreach (var file in dumper.BrokenFiles)
                        ApiConfig.Log.Error($"{file.error}: {file.filename}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Dump is valid");
                    Console.ResetColor();
                }
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }
    }
}
