using System.Text.Json;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Core.Executors;

/// <summary>
/// Calls an external JSON API and returns one value from its response.
/// This is how "weather by latitude and longitude" from the exercise is delivered without writing new code.
/// </summary>
public class HttpExecutor : IOperationExecutor
{
    // Replaces the placeholders only so the address can be parsed and its host checked.
    private const string SampleValue = "0";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpExecutorOptions _options;

    public HttpExecutor(IHttpClientFactory httpClientFactory, HttpExecutorOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public OperationKind Kind => OperationKind.Http;

    public void ValidateDefinition(Operation operation)
    {
        HttpDefinition definition = ParseDefinition(operation);

        CheckAddress(FillPlaceholders(definition.Url, SampleValue, SampleValue));
    }

    public async Task<string> ExecuteAsync(Operation operation, string a, string b, CancellationToken cancellationToken)
    {
        HttpDefinition definition = ParseDefinition(operation);

        if (operation.InputType == InputType.Coordinates)
        {
            CheckCoordinate(a, CoordinateLimits.MaxLatitude, "קו רוחב (A)");
            CheckCoordinate(b, CoordinateLimits.MaxLongitude, "קו אורך (B)");
        }

        string url = FillPlaceholders(definition.Url, a, b);
        CheckAddress(url);

        return await FetchValueAsync(url, definition.Path, cancellationToken);
    }

    private async Task<string> FetchValueAsync(string url, string path, CancellationToken cancellationToken)
    {
        using HttpClient client = _httpClientFactory.CreateClient(ExternalApi.ClientName);
        using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new UserInputException($"השירות החיצוני החזיר {(int)response.StatusCode}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        JsonElement node = document.RootElement;

        foreach (string part in path.Split(ExternalApi.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty(part, out node))
            {
                throw new UserInputException($"הנתיב '{path}' לא נמצא בתשובה");
            }
        }

        string value = node.ValueKind == JsonValueKind.String ? node.GetString()! : node.ToString();

        if (value.Length > InputLimits.MaxResultLength)
        {
            throw new UserInputException("התשובה מהשירות החיצוני ארוכה מדי");
        }

        return value;
    }

    private void CheckAddress(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UserInputException("ה-url חייב להיות כתובת https תקינה");
        }

        if (!_options.AllowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            throw new UserInputException($"הדומיין '{uri.Host}' לא ברשימת הדומיינים המותרים");
        }
    }

    private static string FillPlaceholders(string template, string a, string b)
        => template
            .Replace(OperationParameters.PlaceholderA, Uri.EscapeDataString(a))
            .Replace(OperationParameters.PlaceholderB, Uri.EscapeDataString(b));

    private static HttpDefinition ParseDefinition(Operation operation)
    {
        HttpDefinition? definition = JsonSerializer.Deserialize<HttpDefinition>(operation.Definition, JsonOptions);

        if (definition is null || string.IsNullOrWhiteSpace(definition.Url) || string.IsNullOrWhiteSpace(definition.Path))
        {
            throw new UserInputException("ה-Definition חייב להיות JSON עם url ו-path");
        }

        return definition;
    }

    private static void CheckCoordinate(string text, double limit, string fieldName)
    {
        if (!NumberParser.TryParse(text, out double number) || Math.Abs(number) > limit)
        {
            throw new UserInputException($"{fieldName} חייב להיות מספר בין -{limit} ל-{limit}");
        }
    }
}
