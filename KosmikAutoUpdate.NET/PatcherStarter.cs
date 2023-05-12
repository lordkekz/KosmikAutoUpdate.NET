using System.Diagnostics;

namespace KosmikAutoUpdate.NET;

public class PatcherStarter {
    private readonly string _patcherExecutable;

    public PatcherStarter(string patcherExecutable) { _patcherExecutable = Path.GetFullPath(patcherExecutable); }

    public void StartPatcher(string patchManifestPath) {
        var startInfo =  new ProcessStartInfo(_patcherExecutable);
        Process.Start(startInfo);
    }
}