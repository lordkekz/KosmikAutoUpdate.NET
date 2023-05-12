using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels;

public class LocalManifest {
    [JsonConstructor]
    public LocalManifest(GitSemanticVersion version, string channel,
        GitSemanticVersion? patchTargetVersion, string? patchManifestPath) {
        Version = version;
        Channel = channel;
        PatchTargetVersion = patchTargetVersion;
        PatchManifestPath = patchManifestPath;
    }

    [property: JsonPropertyName("version")]
    public GitSemanticVersion Version { get; set; }

    [property: JsonPropertyName("channel")]
    public string Channel { get; set; }

    [property: JsonPropertyName("patch_target_version")]
    public GitSemanticVersion? PatchTargetVersion { get; set; }

    [property: JsonPropertyName("patch_manifest_path")]
    public string? PatchManifestPath { get; set; }

    [property: JsonIgnore]
    internal Dictionary<string, LocalAppFile> Files { get; private set; } = new();

    internal void PopulateFromLocalFiles(string path) {
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            var hash = Utility.ComputeFileSha256(file);
            var relPath = Path.GetRelativePath(path, file);
            var fileInfo = new FileInfo(file);
            Files[relPath] = new LocalAppFile(relPath, hash, fileInfo.Length);
            Debug.WriteLine(file + " " + hash);
        }
    }
}