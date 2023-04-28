using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using KosmikAutoUpdate.NET.StorageModels;

namespace KosmikAutoUpdate.NET;

public class Updater {
    public string API_Address { get; private set; }
    public GitSemanticVersion CurrentVersion => LocalManifest.Version;
    public string Channel => LocalManifest.Channel;
    public string AppPath { get; private set; }

    internal LocalManifest LocalManifest { get; private init; }

    private readonly ApiClient _client;
    private readonly Downloader _downloader;
    private readonly PatcherStarter _patcherStarter;

    private Updater() {
        _client = new ApiClient("http://localhost:8080/");
        _downloader = new Downloader();
        _patcherStarter =
            new PatcherStarter(
                "../../../../KosmikAutoUpdate.NET.Patcher/bin/Debug/net7.0/KosmikAutoUpdate.NET.Patcher.exe");
    }

    /// <summary>
    /// Main method for updating app.
    /// </summary>
    /// <returns><c>true</c> if the app needs to restart to apply an update; <c>false</c> otherwise</returns>
    public async Task<bool> Update() {
        // TODO remove temp dir after an update
        
        // Get latest version
        var version = await _client.GetVersion(Channel ?? "main");
        Debug.Assert(version is not null);

        // Hash local files
        PopulateManifest();

        // Update decision and analysis logic (stub)
        var updateAvailable = CurrentVersion.CompareTo(version.Version) < 0;

        Debug.WriteLine($"Current version is {
            CurrentVersion
        }; Latest version is {
            version.Version
        }; Update available? {
            updateAvailable
        }");
        if (!updateAvailable) return false;

        var filesToUpdate = FindUpdatedFiles(version).ToList();
        Debug.WriteLine($"Need to download {
            filesToUpdate.Count
        } files with total download size ~{
            filesToUpdate.Sum(f => f.CompressedBytes) / 1000000
        }MB");

        var filesToRemove = FindRemovedFiles(version).ToList();
        Debug.WriteLine($"Need to delete {
            filesToRemove.Count
        } files with total local size ~{
            filesToRemove.Sum(f => f.SizeBytes) / 1000000
        }MB");

        // Download needed files
        var downloaded = _downloader.DownloadAll(filesToUpdate);

        // Create PatchManifest for Patcher
        var updated = filesToUpdate.Select(file =>
            new PatchAppFile(file.RelativePath, new Uri(downloaded[file.FileHash.ToLowerInvariant()])));
        var removed = filesToRemove.Select(file => file.RelativePath);
        // TODO More reliably guess the callbackPath and provide Option to customize it
        // Maybe we can figure out the commandline of our process using the pid?
        var callbackPath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");
        var patchManifest = new PatchManifest(new Uri(AppPath), new Uri(_downloader.TempDirPath),
            updated.ToList(), removed.ToList(), callbackPath);
        var patchManifestPath = Path.Combine(_downloader.TempDirPath, "patch_manifest.json");
        await File.WriteAllTextAsync(patchManifestPath, JsonSerializer.Serialize(patchManifest));

        // TODO Remember patch manifest in local manifest

        // Start Patcher
        _patcherStarter.StartPatcher(patchManifestPath);

        return true;
    }

    /// <summary>
    /// Finds files which are added or changed in <c>target</c> compared to <c>LocalManifest</c>.
    /// </summary>
    /// <param name="target">the <c>RemoteManifest</c> of the target App version</param>
    /// <returns>a filtered sequence of <c>RemoteAppFile</c>s</returns>
    private IEnumerable<RemoteAppFile> FindUpdatedFiles(RemoteManifest target) =>
        from remoteFile in target.Files.Values
        let needsUpdate = !LocalManifest.Files.ContainsKey(remoteFile.RelativePath) ||
                          !LocalManifest.Files[remoteFile.RelativePath].FileHash.Equals(remoteFile.FileHash,
                              StringComparison.InvariantCultureIgnoreCase)
        where needsUpdate
        select remoteFile;

    /// <summary>
    /// Finds files which are removed in <c>target</c> compared to <c>LocalManifest</c>.
    /// </summary>
    /// <param name="target">the <c>RemoteManifest</c> of the target App version</param>
    /// <returns>a filtered sequence of local <c>LocalAppFile</c>s</returns>
    private IEnumerable<LocalAppFile> FindRemovedFiles(RemoteManifest target) =>
        from localFile in LocalManifest.Files.Values
        let wasRemoved = !target.Files.ContainsKey(localFile.RelativePath) && !IsIgnoredFile(localFile)
        where wasRemoved
        select localFile;

    /// <summary>
    /// Determines whether a LocalAppFile is to be ignored by the updater. Ignored files should not be deleted during an update.
    /// </summary>
    /// <param name="localFile"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool IsIgnoredFile(LocalAppFile localFile) => localFile.RelativePath == "kosmikupdate.json";

    private void PopulateManifest() { LocalManifest.PopulateFromLocalFiles(AppPath); }

    public static Updater Create() {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var kosmikPath = Path.Join(path, "kosmikupdate.json");

        var localManifest = JsonSerializer.Deserialize<LocalManifest>(File.ReadAllText(kosmikPath));

        return new Updater {
            AppPath = path!,
            LocalManifest = localManifest
        };
    }
}