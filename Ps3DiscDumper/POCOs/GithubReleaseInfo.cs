using System;
using System.Text.Json.Serialization;

namespace Ps3DiscDumper.POCOs;

public class GitHubReleaseInfo
{
    public string Url { get; set; } = null!;
    public string AssetsUrl { get; set; } = null!;
    public string HtmlUrl { get; set; } = null!;
    public int Id { get; set; }
    public string TagName { get; set; } = null!;
    public string? Name { get; set; }
    public string? Body { get; set; }
    public bool Prerelease { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
[JsonSerializable(typeof(GitHubReleaseInfo[]))]
internal partial class GithubReleaseSerializer: JsonSerializerContext;