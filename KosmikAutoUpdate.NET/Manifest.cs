using System.Diagnostics;

namespace KosmikAutoUpdate.NET;

internal class Manifest {
    public GitSemanticVersion Version { get; private set; }
    public Dictionary<string, AppFile> Files { get; private set; } = new();

    internal static Manifest GenerateFromLocalFiles(GitSemanticVersion version, string path) {
        var manifest = new Manifest();

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            var hash = Helper.ComputeFileSha256(file);
            var relPath = Path.GetRelativePath(path, file);
            manifest.Files[relPath] = new AppFile(relPath, hash);
            Debug.WriteLine(file + " " + hash);
        }

        return manifest;
    }
}

internal record AppFile(string RelativePath, string FileHash) : IAppFile { }

internal interface IAppFile {
    string RelativePath { get; }
    string FileHash { get; }
}