using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels;

internal class LocalManifest {
    [JsonConstructor]
    public LocalManifest(GitSemanticVersion version, string channel) {
        Version = version;
        Channel = channel;
    }

    [property: JsonPropertyName("version")]
    public GitSemanticVersion Version { get; set; }

    [property: JsonPropertyName("channel")]
    public string Channel { get; set; }

    [property: JsonIgnore]
    public Dictionary<string, LocalAppFile> Files { get; private set; } = new();

    internal void PopulateFromLocalFiles(string path) {
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            var hash = Helper.ComputeFileSha256(file);
            var relPath = Path.GetRelativePath(path, file);
            var fileInfo = new FileInfo(file);
            Files[relPath] = new LocalAppFile(relPath, hash, fileInfo.Length);
            Debug.WriteLine(file + " " + hash);
        }
    }
}