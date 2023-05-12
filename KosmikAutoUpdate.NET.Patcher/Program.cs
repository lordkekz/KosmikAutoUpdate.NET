using System.Diagnostics;
using System.Text.Json;
using KosmikAutoUpdate.NET.StorageModels;

public class Program {
    public static void Main(string[] args) {
        Console.Error.WriteLine($"args: {args}");

        try {
            DoMain(args);
        }
        catch (Exception ex) {
            Console.Error.WriteLine(ex);
        }

        // Debug line so the terminal stays open
        while (Console.ReadLine() is null) Thread.Sleep(1000);
    }

    private static void DoMain(string[] args) {
        var localManifestPath = Path.GetFullPath("kosmikupdate.json");
        
        Console.WriteLine($"Reading local manifest from path: {localManifestPath}");
        var localManifest = JsonSerializer.Deserialize<LocalManifest>(File.ReadAllText(localManifestPath));
        
        Console.WriteLine($"Reading patch manifest from path: {localManifest.PatchManifestPath}");
        var patchManifest = JsonSerializer.Deserialize<PatchManifest>(File.ReadAllText(localManifest.PatchManifestPath));
        var absAppPath = patchManifest.AppPath.AbsolutePath;

        Console.WriteLine($"AppPath: {patchManifest.AppPath}");
        Console.WriteLine($"TempDir: {patchManifest.TempDir}");
        Console.WriteLine($"{patchManifest.UpdatedFiles.Count} updated; {patchManifest.RemovedFiles.Count} removed.");

        // Progressively acquire locks to all needed files until everything is locked
        var updatedPaths = patchManifest.UpdatedFiles.Select(f => Path.Combine(absAppPath, f.RelativePath)).ToList();
        var tempPaths = patchManifest.UpdatedFiles.Select(f => f.TempPath.AbsolutePath).ToList();
        var streams = new Dictionary<string, FileStream>();
        while (!AcquireLocks(tempPaths, streams) ||
               !AcquireLocks(updatedPaths, streams) ||
               !AcquireLocks(patchManifest.RemovedFiles, streams)) {
            Console.WriteLine("Still missing some files.");
            Thread.Sleep(1000);
        }
        Console.WriteLine("All files locked.");

        for (var i = 0; i < patchManifest.RemovedFiles.Count; i++) {
            var file = patchManifest.RemovedFiles[i];
            Console.Write($"Removing file {i + 1} of {patchManifest.RemovedFiles.Count}; relative Path '{file}'");
            streams[file].Dispose();
            File.Delete(file);
            Console.WriteLine("    DONE");
        }

        for (var i = 0; i < patchManifest.UpdatedFiles.Count; i++) {
            var file = patchManifest.UpdatedFiles[i];
            var absPath = Path.Combine(absAppPath, file.RelativePath);
            Console.Write($"Updating file {
                i + 1
            } of {
                patchManifest.UpdatedFiles.Count
            }; relative Path '{
                file.RelativePath
            }'");
            var source = streams[file.TempPath.AbsolutePath];
            var target = streams[absPath];
            source.CopyTo(target);
            target.Flush();
            source.Dispose();
            target.Dispose();
            Console.WriteLine("    DONE");
        }

        Console.WriteLine($"All Patches applied.");
        Console.WriteLine($"Running callback: {patchManifest.CallbackPath}");
        Process.Start(patchManifest.CallbackPath);
    }

    public static bool AcquireLocks(IEnumerable<string> paths, Dictionary<string, FileStream> output) {
        var done = true;
        foreach (var path in paths) {
            if (output.ContainsKey(path)) continue;
            try {
                output[path] = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex) {
                Console.Error.WriteLine(ex.Message);
                done = false;
            }
        }

        return done;
    }
}