using System;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using Mono.Options;
using Ps3DiscDumper;

namespace UIConsole
{
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            Log.Info("PS3 Disc Dumper v" + Dumper.Version);
            var lastDiscId = "";
start:
            const string titleBase = "PS3 Disc Dumper";
            var title = titleBase;
            Console.Title = title;
            var output = ".";
            var inDir = "";
            var showHelp = false;
            var options = new OptionSet
            {
                {
                    "i|input=", "Path to the root of blu-ray disc mount", v =>
                    {
                        if (v is string ind)
                            inDir = ind;
                    }
                },
                {
                    "o|output=", "Path to the output folder. Subfolder for each disc will be created automatically", v =>
                    {
                        if (v is string outd)
                            output = outd;
                    }
                },
                {
                    "?|h|help", "Show help", v =>
                    {
                        if (v != null)
                            showHelp = true;
                    },
                    true
                },
            };
            try
            {
                var unknownParams = options.Parse(args);
                if (unknownParams.Count > 0)
                {
                    Log.Warn("Unknown parameters: ");
                    foreach (var p in unknownParams)
                        Log.Warn("\t" + p);
                    showHelp = true;
                }
                if (showHelp)
                {
                    ShowHelp(options);
                    return 0;
                }

                var dumper = new Dumper(ApiConfig.Cts);
                dumper.DetectDisc(inDir);
                await dumper.FindDiscKeyAsync(ApiConfig.IrdCachePath).ConfigureAwait(false);
                if (string.IsNullOrEmpty(dumper.OutputDir))
                {
                    Log.Info("No compatible disc was found, exiting");
                    return 2;
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

                await dumper.DumpAsync(output).ConfigureAwait(false);

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
            catch (OptionException e)
            {
                ShowHelp(options);
                return 1;
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
                    return 0;
                default:
                    goto start;
            }
        }

        private static void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: dotnet run [UI.Console.csproj]");
            Console.WriteLine("Supported arguments:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
