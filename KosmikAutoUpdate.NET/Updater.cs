using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using KosmikAutoUpdate.NET.StorageModels;

namespace KosmikAutoUpdate.NET;

public class Updater {
    public string API_Address { get; private set; }
    public GitSemanticVersion CurrentVersion { get; private set; }

    // Lazily generate local manifest
    internal LocalManifest LocalManifest {
        get {
            if (_localManifest is null) GenerateManifest();
            return _localManifest!;
        }
        private set => _localManifest = value;
    }
    private LocalManifest? _localManifest;

    public string? Channel { get; private set; }
    public string AppPath { get; private set; }
    private ApiClient _client;
    private Downloader _downloader;

    private Updater() {
        _client = new ApiClient("http://localhost:8080/");
        _downloader = new Downloader();
    }

    public async void Update() {
        var version = await _client.GetVersion(Channel ?? "main");
        Debug.Assert(version is not null);

        var filesToUpdate = FindUpdatedFiles(version).ToList();
        Debug.WriteLine($"Need to download {
            filesToUpdate.Count
        } files with total download size {
            filesToUpdate.Sum(f => f.CompressedBytes) / 1000000.0
        }MB");

        var filesToRemove = FindRemovedFiles(version).ToList();
        Debug.WriteLine($"Need to delete {
            filesToRemove.Count
        } files with total local size {
            filesToRemove.Sum(f => f.SizeBytes) / 1000000.0
        }MB");
    }

    /// <summary>
    /// Finds files which are added or changed in <c>target</c> compared to <c>LocalManifest</c>.
    /// </summary>
    /// <param name="target">the <c>RemoteManifest</c> of the target App version</param>
    /// <returns>a filtered sequence of <c>RemoteAppFile</c>s</returns>
    private IEnumerable<RemoteAppFile> FindUpdatedFiles(RemoteManifest target) =>
        from remoteFile in target.Files.Values
        let needsUpdate = !LocalManifest.Files.ContainsKey(remoteFile.RelativePath) ||
                          LocalManifest.Files[remoteFile.RelativePath].FileHash != remoteFile.FileHash
        where needsUpdate
        select remoteFile;

    /// <summary>
    /// Finds files which are removed in <c>target</c> compared to <c>LocalManifest</c>.
    /// </summary>
    /// <param name="target">the <c>RemoteManifest</c> of the target App version</param>
    /// <returns>a filtered sequence of local <c>LocalAppFile</c>s</returns>
    private IEnumerable<LocalAppFile> FindRemovedFiles(RemoteManifest target) =>
        from localFile in LocalManifest.Files.Values
        let wasRemoved = !target.Files.ContainsKey(localFile.RelativePath)
        where wasRemoved
        select localFile;

    private void GenerateManifest() { LocalManifest = LocalManifest.GenerateFromLocalFiles(CurrentVersion, AppPath); }

    public static Updater? Create() {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var kosmikPath = Path.Join(path, "kosmikupdate.json");

        JsonObject root;
        try {
            root = JsonNode.Parse(File.ReadAllText(kosmikPath))!.AsObject();
        }
        catch (JsonException ex) {
            // TODO Handle exception
            throw;
        }

        var currentVersion = new GitSemanticVersion(0, 0, 0, 0);
        try {
            if (root.ContainsKey("version")) {
                var versionTxt = File.ReadAllText(root["version"]!.GetValue<string>());
                currentVersion = new GitSemanticVersion(versionTxt);
            }
        }
        catch (IOException) { }

        string? currentChannel = null;
        if (root.ContainsKey("channel"))
            currentChannel = root["channel"]!.GetValue<string>();

        return new Updater {
            AppPath = path,
            Channel = currentChannel,
            CurrentVersion = currentVersion
        };
    }
}