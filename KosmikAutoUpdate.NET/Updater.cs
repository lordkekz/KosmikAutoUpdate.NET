using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KosmikAutoUpdate.NET;

public class Updater {
    public string API_Address { get; private set; }
    public GitSemanticVersion CurrentVersion { get; private set; }
    public Manifest? LocalManifest { get; private set; }
    public string? Branch { get; private set; }
    public string AppPath { get; private set; }

    private void GenerateManifest() {
        LocalManifest = Manifest.GenerateFromLocalFiles(CurrentVersion, AppPath);
    }

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

        string? currentBranch = null;
        if (root.ContainsKey("channel"))
            currentBranch = root["channel"]!.GetValue<string>();

        return new Updater {
            AppPath = path,
            Branch = currentBranch,
            CurrentVersion = currentVersion
        };
    }
}