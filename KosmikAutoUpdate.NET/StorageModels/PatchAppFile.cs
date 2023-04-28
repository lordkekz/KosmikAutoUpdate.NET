using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels; 

public record PatchAppFile(
    [property: JsonPropertyName("relative_path")]
    string RelativePath,
    [property: JsonPropertyName("temp_path")]
    Uri TempPath) { }