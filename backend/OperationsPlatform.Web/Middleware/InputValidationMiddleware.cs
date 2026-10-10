using System.Text.Json;
using System.Text.RegularExpressions;
using OperationsPlatform.Web.Handlers;
using OperationsPlatform.Web.Routes;

namespace OperationsPlatform.Web.Middleware;

/// <summary>
/// Checks the body of every POST and PUT to /api before any controller runs.
/// (A body larger than Kestrel:Limits:MaxRequestBodySize never gets here: Kestrel answers 413.) Every string in the JSON (property names and values, at any depth)
/// must pass the allow-list regex below; if one does not, the caller gets a plain 400 with no details
/// and the field that failed goes to the log. The rules of each field (length, format) are FluentValidation's job, in the services.
/// A body that is not valid JSON is passed on, and model binding answers it with 400.
/// </summary>
public class InputValidationMiddleware
{
    /// <summary>
    /// One allow-list for every piece of text a caller sends. Text is accepted only if EVERY character is one of:
    /// letters of any language (Hebrew included), combining marks, digits, punctuation, space, tab, line breaks,
    /// the symbols + = ^ | ~ $ and the degree sign, and the two invisible Hebrew direction marks.
    /// Everything else is refused: the characters &lt; and &gt; (so no HTML tag can be written), control characters,
    /// direction overrides, emoji and other symbols. Listing what is allowed is safer than listing what is forbidden,
    /// because a new kind of attack is refused by default.
    /// Known limits: a formula cannot use &lt; or &gt; (use Max, Min, Sign or Abs instead), and plain text such as "javascript:"
    /// is allowed because it is harmless until someone places it in a link, which this platform never does.
    /// The timeout protects the server from input built to make a pattern run for a very long time.
    /// </summary>
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

                // A plain answer with no explanation: a caller sending forbidden characters is not told which rule caught them.
                await ProblemResponse.WriteAsync(context, StatusCodes.Status400BadRequest, ProblemTitles.InvalidRequest);
                return;
            }
        }

        await _next(context);
    }

    private static bool HasBodyToCheck(HttpRequest request) =>
        (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method)) && request.Path.StartsWithSegments(ApiRoutes.Api);

    /// <summary>Returns the path of the first string that fails the allow-list, or null when all pass or the body is not JSON.</summary>
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
