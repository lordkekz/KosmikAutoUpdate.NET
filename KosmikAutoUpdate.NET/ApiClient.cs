using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KosmikAutoUpdate.NET;

internal class ApiClient : IDisposable {
    private readonly HttpClient _client = new();

    public string ApiAddress { get; private set; }

    internal ApiClient(string apiAddress) {
        ApiAddress = apiAddress;
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "KosmikAutoUpdate.NET");
        _client.BaseAddress = new Uri(apiAddress);
    }

    public void Dispose() {
        _client.Dispose();
    }

    internal async Task<Dictionary<string, GitSemanticVersion>?> GetChannels() {
        using var response = await _client.PostAsJsonAsync("get_channels", new Dictionary<string, object>());
        var dict = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, GitSemanticVersion>>>();
        if (dict is null || !dict.ContainsKey("channels"))
            return null;
        return dict["channels"];
    }

    internal async Task<RemoteManifest?> GetVersion(string channel) {
        var rq = new Dictionary<string, object> { { "channel", channel } };
        return await DoGetVersion(rq);
    }

    internal async Task<RemoteManifest?> GetVersion(GitSemanticVersion version) {
        var rq = new Dictionary<string, object> { { "version_id", version.ToString() } };
        return await DoGetVersion(rq);
    }

    private async Task<RemoteManifest?> DoGetVersion(Dictionary<string, object> rq) {
        using var response = await _client.PostAsJsonAsync("get_version", rq);
        // return await response.Content.ReadFromJsonAsync<RemoteManifest>();
        var str = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(str);
        return JsonSerializer.Deserialize<RemoteManifest>(str);
    }
}