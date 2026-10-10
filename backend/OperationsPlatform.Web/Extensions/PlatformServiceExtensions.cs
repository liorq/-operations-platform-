using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core;
using OperationsPlatform.Web.Handlers;

namespace OperationsPlatform.Web.Extensions;

public static class PlatformServiceExtensions
{
    private const int LoggedBodyCharacters = 4000;
    private const long MaxRequestBodyBytes = 65536;

    /// <summary>
    /// Layers: Web uses Core (the business logic and data access), which uses Domain. Common holds what all of them share.
    /// Error handling and request logging are the ASP.NET built-ins, configured here.
    /// </summary>
    public static IServiceCollection AddPlatform(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConfigKeys.ConnectionStringName);
        string[] allowedHosts = configuration.GetSection(ConfigKeys.AllowedHttpHosts).Get<string[]>() ?? Array.Empty<string>();

        services.AddCore(connectionString, allowedHosts);

        services.AddControllers().AddJsonOptions(ConfigureJson);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHttpLogging(ConfigureHttpLogging);
        services.Configure<KestrelServerOptions>(ConfigureKestrel);
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>Enums travel as text ("Expression") instead of numbers (1), so the API is readable.</summary>
    private static void ConfigureJson(JsonOptions options) =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    /// <summary>A larger body never reaches our code: Kestrel answers 413.</summary>
    private static void ConfigureKestrel(KestrelServerOptions options) =>
        options.Limits.MaxRequestBodySize = MaxRequestBodyBytes;

    /// <summary>One log entry per request that holds the request, the response, the status and the duration.</summary>
    private static void ConfigureHttpLogging(HttpLoggingOptions options)
    {
        options.LoggingFields = HttpLoggingFields.RequestMethod
            | HttpLoggingFields.RequestPath
            | HttpLoggingFields.RequestQuery
            | HttpLoggingFields.RequestBody
            | HttpLoggingFields.ResponseStatusCode
            | HttpLoggingFields.ResponseBody
            | HttpLoggingFields.Duration;

        options.RequestBodyLogLimit = LoggedBodyCharacters;
        options.ResponseBodyLogLimit = LoggedBodyCharacters;
        options.CombineLogs = true;
    }
}
