using System.Diagnostics;

namespace KosmikAutoUpdate.NET;

public class Manifest {
    public GitSemanticVersion Version { get; private set; }
    public List<AppFile> Files { get; private set; } = new();

    internal static Manifest GenerateFromLocalFiles(GitSemanticVersion version, string path) {
        var manifest = new Manifest();

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            var hash = Helper.ComputeFileSha256(file);
            var relPath = Path.GetRelativePath(path, file);
            manifest.Files.Add(new AppFile(relPath, hash));
            Debug.WriteLine(file + " " + hash);
        }

        return manifest;
    }
}

public record AppFile(string RelativePath, string FileHash) { }