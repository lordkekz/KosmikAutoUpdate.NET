using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET.StorageModels;

internal record LocalAppFile(
    [property: JsonIgnore]
    string RelativePath,
    [property: JsonPropertyName("file_hash")]
    string FileHash,
    [property: JsonIgnore]
    long SizeBytes) : IAppFile { }