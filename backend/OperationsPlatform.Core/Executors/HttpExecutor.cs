using System.Text.Json;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Core.Executors;

public class HttpExecutor : IOperationExecutor
{
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
            CheckCoordinate(a, CoordinateLimits.MaxLatitude, "Latitude (A)");
            CheckCoordinate(b, CoordinateLimits.MaxLongitude, "Longitude (B)");
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
            throw new ExternalServiceException($"The external service returned {(int)response.StatusCode}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        JsonElement node = document.RootElement;

        foreach (string part in path.Split(ExternalApi.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty(part, out node))
            {
                throw new ExternalServiceException($"The path '{path}' was not found in the response");
            }
        }

        string value = node.ValueKind == JsonValueKind.String ? node.GetString()! : node.ToString();

        if (value.Length > InputLimits.MaxResultLength)
        {
            throw new ExternalServiceException("The external service response is too long");
        }

        return value;
    }

    private void CheckAddress(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UserInputException("The url must be a valid https address");
        }

        if (!_options.AllowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            throw new UserInputException($"The domain '{uri.Host}' is not in the allowed domains list");
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
            throw new UserInputException("The definition must be JSON with url and path");
        }

        return definition;
    }

    private static void CheckCoordinate(string text, double limit, string fieldName)
    {
        if (!NumberParser.TryParse(text, out double number) || Math.Abs(number) > limit)
        {
            throw new UserInputException($"{fieldName} must be a number between -{limit} and {limit}");
        }
    }
}
