using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels; 

public class PatchManifest {
    [JsonConstructor]
    public PatchManifest(Uri appPath, Uri tempDir, List<PatchAppFile> updatedFiles, List<string> removedFiles) {
        AppPath = appPath;
        TempDir = tempDir;
        UpdatedFiles = updatedFiles;
        RemovedFiles = removedFiles;
    }

    [property: JsonPropertyName("app_path")]
    public Uri AppPath { get; private set; }

    [property: JsonPropertyName("temp_dir")]
    public Uri TempDir { get; private set; }

    [property: JsonPropertyName("updated_files")]
    public List<PatchAppFile> UpdatedFiles { get; private set; }

    [property: JsonPropertyName("removed_files")]
    public List<string> RemovedFiles { get; private set; }
}