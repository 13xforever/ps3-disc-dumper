using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IrdLibraryClient;

namespace Ps3DiscDumper
{
    public class Dumper
    {
        public void Dump(string input, string output)
        {
            if (!Directory.Exists(input))
                throw new ArgumentException($"Directory not found: {input}", nameof(input));

            var discSfbPath = Path.Combine(input, "PS3_DISC.SFB");
            if (!File.Exists(discSfbPath))
                throw new InvalidOperationException($"Specified folder is not a PS3 disc root: {input}");

            var discSfbData = File.ReadAllBytes(discSfbPath);
            var sfb = SfbReader.Parse(discSfbData);

            var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? new char[0]);
            ApiConfig.Log.Debug($"Disc flags are {string.Concat(flags)}");
            if (!flags.Contains('g'))
                ApiConfig.Log.Warn("Disc is not a game disc");
            var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
            ApiConfig.Log.Debug($"Disc product code is {titleId}");

            output += $"[{titleId}] Game Title";
            //if (!Directory.Exists(output))
            //    Directory.CreateDirectory(output);
        }
    }
}
