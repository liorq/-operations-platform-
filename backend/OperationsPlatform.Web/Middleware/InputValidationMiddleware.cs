using System.Text.Json;
using System.Text.RegularExpressions;
using OperationsPlatform.Web.Handlers;
using OperationsPlatform.Web.Routes;

namespace OperationsPlatform.Web.Middleware;

public class InputValidationMiddleware
{
    private const string RootPath = "body";
    private const int RegexTimeoutMilliseconds = 100;

    private static readonly Regex Allowed = new Regex(
        @"^[\p{L}\p{M}\p{N}\p{P} \t\r\n+=^|~$°\u200e\u200f]*\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));

    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HasBodyToCheck(context.Request))
        {
            context.Request.EnableBuffering();
            string? disallowedField = await FindDisallowedFieldAsync(context.Request.Body);
            context.Request.Body.Position = 0;

            if (disallowedField is not null)
            {
                _logger.LogWarning(
                    "Rejected {Method} {Path}: field '{Field}' has characters that are not allowed",
                    context.Request.Method,
                    context.Request.Path,
                    disallowedField.ReplaceLineEndings(" "));

                await ProblemResponse.WriteAsync(context, StatusCodes.Status400BadRequest, ProblemTitles.InvalidRequest);
                return;
            }
        }

        await _next(context);
    }

    private static bool HasBodyToCheck(HttpRequest request) =>
        (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method)) && request.Path.StartsWithSegments(ApiRoutes.Api);

    private static async Task<string?> FindDisallowedFieldAsync(Stream body)
    {
        try
        {
            using JsonDocument document = await JsonDocument.ParseAsync(body);

            return FindDisallowedField(document.RootElement, RootPath);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FindDisallowedField(JsonElement element, string path)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                string childPath = path + "." + property.Name;
                string? found = Allowed.IsMatch(property.Name) ? FindDisallowedField(property.Value, childPath) : childPath;

                if (found is not null)
                {
                    return found;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                string? found = FindDisallowedField(item, path);

                if (found is not null)
                {
                    return found;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.String && !Allowed.IsMatch(element.GetString() ?? string.Empty))
        {
            return path;
        }

        return null;
    }
}
