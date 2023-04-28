using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using KosmikAutoUpdate.NET.StorageModels;

public class Program {
    public static void Main(string[] args) {
        if (args.Length != 1)
            Console.Error.WriteLine($"Expected exactly one argument (the patch manifest path); got {args.Length}!");

        var manifestPath = args[0];
        var patchManifest = JsonSerializer.Deserialize<PatchManifest>(File.ReadAllText(manifestPath));
        var absAppPath = patchManifest.AppPath.AbsolutePath;
        
        Console.WriteLine($"Read manifest from path: {manifestPath}");
        Console.WriteLine($"AppPath: {patchManifest.AppPath}");
        Console.WriteLine($"TempDir: {patchManifest.TempDir}");
        Console.WriteLine($"{patchManifest.UpdatedFiles.Count} updated; {patchManifest.RemovedFiles.Count} removed.");
        
        for (int i = 0; i < patchManifest.RemovedFiles.Count; i++) {
            var file = patchManifest.RemovedFiles[i];
            Console.Write($"Removing file {i+1} of {patchManifest.RemovedFiles.Count}; relative Path '{file}'");
            File.Delete(file);
            Console.WriteLine("    DONE");
        }
        
        for (int i = 0; i < patchManifest.UpdatedFiles.Count; i++) {
            var file = patchManifest.UpdatedFiles[i];
            var absPath = Path.Combine(absAppPath, file.RelativePath);
            Console.Write($"Updating file {i+1} of {patchManifest.UpdatedFiles.Count}; relative Path '{file}'");
            File.Copy(file.TempPath.AbsolutePath, absPath, true);
            Console.WriteLine("    DONE");
        }
        
        Console.WriteLine($"All Patches applied.");
        // TODO Callback
    }
}