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

    private static void ConfigureJson(JsonOptions options) =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    private static void ConfigureKestrel(KestrelServerOptions options) =>
        options.Limits.MaxRequestBodySize = MaxRequestBodyBytes;

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
