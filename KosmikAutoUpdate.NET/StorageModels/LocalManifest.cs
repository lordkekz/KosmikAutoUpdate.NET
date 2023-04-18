using System.Diagnostics;

namespace KosmikAutoUpdate.NET.StorageModels;

internal class LocalManifest {
    public GitSemanticVersion Version { get; private set; }
    public Dictionary<string, LocalAppFile> Files { get; private set; } = new();

    internal static LocalManifest GenerateFromLocalFiles(GitSemanticVersion version, string path) {
        var manifest = new LocalManifest();

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            var hash = Helper.ComputeFileSha256(file);
            var relPath = Path.GetRelativePath(path, file);
            var fileInfo = new FileInfo(file);
            manifest.Files[relPath] = new LocalAppFile(relPath, hash, fileInfo.Length);
            Debug.WriteLine(file + " " + hash);
        }

        return manifest;
    }
}