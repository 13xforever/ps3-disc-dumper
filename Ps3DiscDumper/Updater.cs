using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.DiscInfo;
using Ps3DiscDumper.DiscKeyProviders;
using Ps3DiscDumper.POCOs;
using Ps3DiscDumper.Sfb;
using Ps3DiscDumper.Sfo;
using Ps3DiscDumper.Utils;
using Ps3DiscDumper.Utils.MacOS;
using WmiLight;
using FileInfo = System.IO.FileInfo;

namespace Ps3DiscDumper;

public static partial class Updater
{
    [GeneratedRegex(@"(?<ver>\d+(\.\d+){0,2})[ \-]*(?<pre>.*)", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex VersionParts();

    public static async Task<(Version?, GitHubReleaseInfo?)> CheckUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            var versionParts = VersionParts();
            var curVerMatch = versionParts.Match(Dumper.Version);
            var curVerStr = curVerMatch.Groups["ver"].Value;
            var curVerPre = curVerMatch.Groups["pre"].Value;
            client.DefaultRequestHeaders.UserAgent.Add(new("PS3DiscDumper", curVerStr));
            var responseJson = await client.GetStringAsync("https://api.github.com/repos/13xforever/ps3-disc-dumper/releases").ConfigureAwait(false);
            var releaseList = JsonSerializer.Deserialize(responseJson, GithubReleaseSerializer.Default.GitHubReleaseInfoArray)?
                .OrderByDescending(r => Version.TryParse(r.TagName.TrimStart('v'), out var v) ? v : null)
                .ToList();
            var latest = releaseList?.FirstOrDefault(r => !r.Prerelease);
            var latestBeta = releaseList?.FirstOrDefault(r => r.Prerelease);
            _ = Version.TryParse(curVerStr, out var curVer);
            _ = Version.TryParse(latest?.TagName.TrimStart('v') ?? "0", out var latestVer);
            var latestBetaMatch = versionParts.Match(latestBeta?.TagName ?? "");
            var latestBetaVerStr = latestBetaMatch.Groups["ver"].Value;
            var latestBetaVerPre = latestBetaMatch.Groups["pre"].Value;
            _ = Version.TryParse("0" + latestBetaVerStr, out var latestBetaVer);
            if (latestVer > curVer || latestVer == curVer && curVerPre is {Length: >0})
            {
                Log.Warn($"Newer version available: v{latestVer}\n\n{latest?.Name}\n{"".PadRight(latest?.Name?.Length ?? 0, '-')}\n{latest?.Body}\n{latest?.HtmlUrl}\n");
                return (latestVer, latest);
            }
                
            if (latestBetaVer > curVer
                || (latestBetaVer == curVer
                    && curVerPre is {Length: >0}
                    && (latestBetaVerPre is {Length: >0} && StringComparer.OrdinalIgnoreCase.Compare(latestBetaVerPre, curVerPre) > 0
                        || latestBetaVerStr is null or "")))
            {
                Log.Warn($"Newer prerelease version available: v{latestBeta!.TagName.TrimStart('v')}\n\n{latestBeta?.Name}\n{"".PadRight(latestBeta?.Name.Length ?? 0, '-')}\n{latestBeta?.Body}\n{latestBeta?.HtmlUrl}\n");
                return (latestBetaVer, latestBeta);
            }
        }
        catch
        {
            Log.Warn("Failed to check for updates");
        }
        return (null, null);
    }
}