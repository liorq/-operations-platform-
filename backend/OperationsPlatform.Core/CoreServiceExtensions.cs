using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core.Abstractions;
using OperationsPlatform.Core.Data;
using OperationsPlatform.Core.Executors;
using OperationsPlatform.Core.Services.Calculation;
using OperationsPlatform.Core.Services.Operations;
using OperationsPlatform.Core.Services.Resolving;
using OperationsPlatform.Core.Time;
using OperationsPlatform.Core.Validators;

namespace OperationsPlatform.Core;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, string? connectionString, string[] allowedHosts)
    {
        services.AddSingleton<TimeProvider, IsraelTimeProvider>();

        services.AddDbContext<OperationsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationLogRepository, OperationLogRepository>();

        services.AddSingleton(new HttpExecutorOptions(allowedHosts));
        services.AddHttpClient(ExternalApi.ClientName, ConfigureExternalClient)
            .ConfigurePrimaryHttpMessageHandler(CreateHandlerWithoutRedirects);

        services.AddScoped<IOperationExecutor, ExpressionExecutor>();
        services.AddScoped<IOperationExecutor, TextExecutor>();
        services.AddScoped<IOperationExecutor, HttpExecutor>();
        services.AddScoped<IExecutorResolver, ExecutorResolver>();

        services.AddValidatorsFromAssemblyContaining<CalculateRequestValidator>();
        ValidatorOptions.Global.PropertyNameResolver = ToCamelCaseName;

        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<IOperationService, OperationService>();

        return services;
    }

    private static string? ToCamelCaseName(Type type, MemberInfo? member, LambdaExpression? expression) =>
        member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

    private static void ConfigureExternalClient(HttpClient client) => client.Timeout = TimeSpan.FromSeconds(ExternalApi.TimeoutSeconds);

    private static HttpMessageHandler CreateHandlerWithoutRedirects() => new HttpClientHandler { AllowAutoRedirect = false };
}
