using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET;

internal class RemoteAppFile : IAppFile {
    public RemoteAppFile(string fileHash, long compressedBytes, string fileUrl, string relativePath = null) {
        RelativePath = relativePath;
        FileHash = fileHash;
        CompressedBytes = compressedBytes;
        FileUrl = fileUrl;
    }

    [property: JsonIgnore]
    public string RelativePath { get; internal set; }

    [property: JsonPropertyName("file_hash")]
    public string FileHash { get; }

    [property: JsonPropertyName("compressed_bytes")]
    public long CompressedBytes { get; }

    [property: JsonPropertyName("file_url")]
    public string FileUrl { get; }
}