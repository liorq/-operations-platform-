using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OperationsPlatform.Tests.Support;

/// <summary>Sends and reads JSON the same way the API does: camelCase names and enums as text.</summary>
public static class TestJson
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body, string? adminKey = null) =>
        SendAsync(client, url, JsonContent.Create(body, options: Options), adminKey);

    /// <summary>Posts the text as it is, so a test can send broken or hostile JSON.</summary>
    public static Task<HttpResponseMessage> PostRawAsync(HttpClient client, string url, string json, string? adminKey = null) =>
        SendAsync(client, url, new StringContent(json, Encoding.UTF8, "application/json"), adminKey);

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, string url, HttpContent content, string? adminKey)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

        if (adminKey is not null)
        {
            request.Headers.Add("X-Admin-Key", adminKey);
        }

        return client.SendAsync(request);
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Options))!;
}
