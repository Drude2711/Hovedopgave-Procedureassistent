using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Common.Authentication;
using Common.Models;
using Common.Utilities;

namespace Common.Providers;

internal class GraphProvider
{
    private static readonly HttpClient _http = new() { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
    private static readonly HttpClient _download = new();
    private static string? _siteId;

    internal async Task<ProcedureDocument> GetDocumentAsync(string fileName)
    {
        string siteId = await GetSiteIdAsync();

        using JsonDocument item = await GetJsonAsync(
            $"sites/{siteId}/drive/root:/{Uri.EscapeDataString(fileName).Replace("%2F", "/")}");

        string itemId = item.RootElement.GetProperty("id").GetString()!;
        string webUrl = item.RootElement.GetProperty("webUrl").GetString()!;

        byte[] bytes = item.RootElement.TryGetProperty("@microsoft.graph.downloadUrl", out JsonElement downloadUrl)
            ? await DownloadWithRetryAsync(downloadUrl.GetString()!)
            : await DownloadContentAsync(siteId, itemId);
        using var memory = new MemoryStream(bytes);

        return new ProcedureDocument
        {
            Name = Path.GetFileName(fileName),
            Url = webUrl,
            Text = WordTextExtractor.Extract(memory)
        };
    }

    private static async Task<byte[]> DownloadWithRetryAsync(string url)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await _download.GetByteArrayAsync(url);
            }
            catch (HttpRequestException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }
    }

    private static async Task<byte[]> DownloadContentAsync(string siteId, string itemId)
    {
        using HttpRequestMessage request = await CreateRequestAsync($"sites/{siteId}/drive/items/{itemId}/content");
        using HttpResponseMessage response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<string> GetSiteIdAsync()
    {
        if (_siteId is not null)
        {
            return _siteId;
        }

        using JsonDocument site = await GetJsonAsync($"sites/{AppSettings.SharePointHostname}:{AppSettings.SharePointSitePath}");
        _siteId = site.RootElement.GetProperty("id").GetString()!;
        return _siteId;
    }

    private async Task<JsonDocument> GetJsonAsync(string path)
    {
        using HttpRequestMessage request = await CreateRequestAsync(path);
        using HttpResponseMessage response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static async Task<HttpRequestMessage> CreateRequestAsync(string path)
    {
        AccessToken token = await Authenticator.GetGraphCredential()
            .GetTokenAsync(new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }), CancellationToken.None);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return request;
    }
}
