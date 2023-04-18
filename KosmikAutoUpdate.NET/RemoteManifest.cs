using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET;

internal class RemoteManifest {
    [JsonConstructor]
    public RemoteManifest(GitSemanticVersion version, DateTime date, long archiveBytes, string archiveHash,
        string archiveUrl, Dictionary<string, RemoteAppFile> files) {
        Version = version;
        Date = date;
        ArchiveBytes = archiveBytes;
        ArchiveHash = archiveHash;
        ArchiveUrl = archiveUrl;
        Files = files;
        foreach (var (key, val) in files) val.RelativePath = key;
    }

    [property: JsonPropertyName("version_id")]
    public GitSemanticVersion Version { get; }

    [property: JsonPropertyName("date")]
    public DateTime Date { get; }

    [property: JsonPropertyName("archive_bytes")]
    public long ArchiveBytes { get; }

    [property: JsonPropertyName("archive_hash")]
    public string ArchiveHash { get; }

    [property: JsonPropertyName("archive_url")]
    public string ArchiveUrl { get; }

    [property: JsonPropertyName("files")]
    public Dictionary<string, RemoteAppFile> Files { get; }
}