using System.Security.Cryptography;
using OperationsPlatform.Common.Constants;
using System.Text;
using OperationsPlatform.Web.Handlers;
using OperationsPlatform.Web.Routes;

namespace OperationsPlatform.Web.Middleware;

public class AdminKeyMiddleware
{
    private const string AdminKeyHeader = "X-Admin-Key";

    private readonly RequestDelegate _next;
    private readonly byte[] _keyHash;

    public AdminKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        string key = configuration[ConfigKeys.AdminKey] ?? string.Empty;
        _keyHash = key.Length == 0 ? Array.Empty<byte>() : Hash(key);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (NeedsKey(context.Request) && !HasValidKey(context.Request))
        {
            await ProblemResponse.WriteAsync(context, StatusCodes.Status401Unauthorized, ProblemTitles.AdminKeyRequired);
            return;
        }

        await _next(context);
    }

    private static bool NeedsKey(HttpRequest request)
        => request.Path.StartsWithSegments(ApiRoutes.Operations)
            && (!(HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
                || request.Query.ContainsKey(ApiRoutes.IncludeInactiveQuery));

    private bool HasValidKey(HttpRequest request)
    {
        if (_keyHash.Length == 0)
        {
            return false;
        }

        string givenKey = request.Headers[AdminKeyHeader].ToString();

        return CryptographicOperations.FixedTimeEquals(Hash(givenKey), _keyHash);
    }

    private static byte[] Hash(string text) => SHA256.HashData(Encoding.UTF8.GetBytes(text));
}
