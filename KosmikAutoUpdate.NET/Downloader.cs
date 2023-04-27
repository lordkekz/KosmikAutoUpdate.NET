using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using KosmikAutoUpdate.NET.StorageModels;

namespace KosmikAutoUpdate.NET;

internal class Downloader : IDisposable {
    private readonly HttpClient _client = new();

    public DirectoryInfo TempDir { get; }
    public string TempDirPath => TempDir.FullName;

    public Downloader() {
        TempDir = Directory.CreateTempSubdirectory("Kosmik_");
        Debug.WriteLine(TempDirPath);
        Directory.CreateDirectory(TempDirDownloadsPath);
        Directory.CreateDirectory(TempDirFilesPath);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "KosmikAutoUpdate.NET");
    }

    public void Dispose() { _client.Dispose(); }

    public Dictionary<string, string> DownloadAll(List<RemoteAppFile> filesToUpdate) {
        var downloadedFiles = new Dictionary<string, string>();
        foreach (var file in filesToUpdate) {
            if (downloadedFiles.ContainsKey(file.FileHash)) continue;
            var tempFileDLPath = DownloadAsync(file).Result;
            var tempFileExtractedPath = ExtractFile(file, tempFileDLPath);
            var valid = VerifyFileHash(file, tempFileExtractedPath);
            Debug.Assert(valid);
            downloadedFiles[file.FileHash] = tempFileExtractedPath;
        }

        return downloadedFiles;
    }

    private async Task<string> DownloadAsync(RemoteAppFile file) {
        var urlFileName = Path.GetFileName(file.FileUrl.AbsolutePath);
        var tempFileDLPath = Path.Join(TempDirDownloadsPath, urlFileName);
        Debug.Assert(file.FileHash + ".zip" == urlFileName);

        var response = await _client.GetAsync(file.FileUrl);
        await using var fs = File.OpenWrite(tempFileDLPath);
        await response.Content.CopyToAsync(fs);
        return tempFileDLPath;
    }

    public string TempDirDownloadsPath => Path.Join(TempDirPath, "downloads");
    public string TempDirFilesPath => Path.Join(TempDirPath, "files");

    private string ExtractFile(RemoteAppFile file, string tempFileDLPath) {
        var tempFileExtractedPath = Path.Join(TempDirPath, "files", file.FileHash.ToLowerInvariant());
        using var zip = ZipFile.OpenRead(tempFileDLPath);
        var entry = zip.GetEntry(file.FileHash);
        Debug.Assert(entry is not null);
        entry.ExtractToFile(tempFileExtractedPath);
        return tempFileExtractedPath;
    }

    private bool VerifyFileHash(RemoteAppFile file, string tempFileExtractedPath) =>
        file.FileHash.Equals(Helper.ComputeFileSha256(tempFileExtractedPath),
            StringComparison.InvariantCultureIgnoreCase);
}