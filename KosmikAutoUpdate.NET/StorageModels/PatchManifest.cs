using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels; 

public class PatchManifest {
    [JsonConstructor]
    public PatchManifest(List<PatchAppFile> files, Uri appPath, Uri tempDir) {
        Files = files;
        AppPath = appPath;
        TempDir = tempDir;
    }

    [property: JsonPropertyName("app_path")]
    public Uri AppPath { get; private set; }

    [property: JsonPropertyName("temp_dir")]
    public Uri TempDir { get; private set; }

    [property: JsonPropertyName("files")]
    public List<PatchAppFile> Files { get; private set; }
}