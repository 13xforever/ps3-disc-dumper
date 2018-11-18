using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.POCOs;
using Ps3DiscDumper.Sfb;
using Ps3DiscDumper.Sfo;

namespace Ps3DiscDumper
{
    public class Dumper
    {
        private static readonly IrdClient Client = new IrdClient();
        private static readonly HashSet<char> InvalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        public async Task DumpAsync(string input, string output, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(input))
                throw new ArgumentException($"Directory not found: {input}", nameof(input));

            var discSfbPath = Path.Combine(input, "PS3_DISC.SFB");
            if (!File.Exists(discSfbPath))
                throw new InvalidOperationException($"Specified folder is not a PS3 disc root (ps3_disc.sfb is missing): {input}");

            var discSfbData = File.ReadAllBytes(discSfbPath);
            var sfb = SfbReader.Parse(discSfbData);

            var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? new char[0]);
            ApiConfig.Log.Debug($"Disc flags are {string.Concat(flags)}");
            if (!flags.Contains('g'))
                ApiConfig.Log.Warn("Disc is not a game disc");
            var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
            ApiConfig.Log.Debug($"Disc product code is {titleId}");
            if (string.IsNullOrEmpty(titleId))
                ApiConfig.Log.Warn("Disc product code is empty");
            else if (titleId.Length > 9)
                titleId = titleId.Substring(0, 4) + titleId.Substring(titleId.Length - 5, 5);
            var paramSfoPath = Path.Combine(input, "PS3_GAME", "PARAM.SFO");
            if (!File.Exists(paramSfoPath))
                throw new InvalidOperationException($"Specified folder is not a valid PS3 disc root (param.sfo is missing): {input}");

            ParamSfo sfo;
            using (var stream = File.Open(paramSfoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                sfo = ParamSfo.ReadFrom(stream);
            var updateVer = sfo.Items.FirstOrDefault(i => i.Key == "PS3_SYSTEM_VER")?.StringValue?.Substring(0, 5).TrimStart('0').Trim();
            var gameVer = sfo.Items.FirstOrDefault(i => i.Key == "VERSION")?.StringValue?.Substring(0, 5).Trim();
            var appVer = sfo.Items.FirstOrDefault(i => i.Key == "APP_VER")?.StringValue?.Substring(0, 5).Trim();
            var title = sfo.Items.FirstOrDefault(i => i.Key == "TITLE")?.StringValue?.Trim(' ', '\0');
            var titleIdSfo = sfo.Items.FirstOrDefault(i => i.Key == "TITLE_ID")?.StringValue?.Trim(' ', '\0');

            ApiConfig.Log.Debug($"Game version: {gameVer}");
            ApiConfig.Log.Debug($"App version: {appVer}");
            ApiConfig.Log.Debug($"Update version: {updateVer}");
            var outputDir = new string($"[{titleIdSfo}] {title}".ToCharArray().Where(c => !InvalidChars.Contains(c)).ToArray());
            ApiConfig.Log.Info(outputDir);
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(titleId, titleIdSfo))
                ApiConfig.Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({titleIdSfo}) do not match");

            var irdInfoList = await Client.SearchAsync(titleIdSfo, cancellationToken).ConfigureAwait(false);
            var irdList = irdInfoList.Data?.Where(i => i.Filename.Substring(0, 9).ToUpperInvariant() == titleIdSfo?.ToUpperInvariant()
                                                       && i.AppVersion == appVer
                                                       && i.UpdateVersion == updateVer
                                                       && i.GameVersion == gameVer
                          ).ToList() ?? new List<SearchResultItem>(0);
            if (irdList.Count == 0)
            {
                ApiConfig.Log.Error("No matching IRD file was found");
                return;
            }

            if (irdList.Count > 1)
            {
                ApiConfig.Log.Warn($"{irdList.Count} matching IRD files were found???");
                return;
            }
            var ird = await Client.DownloadAsync(irdList[0], "ird/", cancellationToken).ConfigureAwait(false);
            if (ird == null)
            {
                ApiConfig.Log.Error("No valid matching IRD file could be loaded");
                return;
            }

            output = Path.Combine(output, outputDir);
            //if (!Directory.Exists(output))
            //    Directory.CreateDirectory(output);
        }
    }
}
