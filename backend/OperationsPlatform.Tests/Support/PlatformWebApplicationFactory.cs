using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OperationsPlatform.Tests.Support;

/// <summary>
/// Starts the whole application in memory: real controllers, real middleware, real services.
/// Only the outside world is replaced: SQL Server becomes in-memory SQLite and the weather service becomes a stub.
/// </summary>
public class PlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminKey = "test-admin-key";

    private const string WeatherJson = "{\"current\":{\"temperature_2m\":23.5}}";

    private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

    public PlatformWebApplicationFactory()
    {
        _connection.Open();
    }

    public StubHttpMessageHandler WeatherService { get; } = new StubHttpMessageHandler(WeatherJson);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(ConfigKeys.AutoMigrate, "false");
        builder.UseSetting(ConfigKeys.AdminKey, AdminKey);

        builder.ConfigureTestServices(ReplaceOutsideWorld);
    }

    private void ReplaceOutsideWorld(IServiceCollection services)
    {
        // Remove everything AddDbContext registered for SQL Server, then register SQLite on the same open connection.
        RemoveDescriptorsOf(services);
        services.AddDbContext<OperationsDbContext>(ConfigureSqlite);

        services.AddHttpClient(ExternalApi.ClientName).ConfigurePrimaryHttpMessageHandler(CreateWeatherStub);
    }

    /// <summary>Removes every registration that AddDbContext created for OperationsDbContext (its options and their configuration).</summary>
    private static void RemoveDescriptorsOf(IServiceCollection services)
    {
        for (int index = services.Count - 1; index >= 0; index--)
        {
            Type serviceType = services[index].ServiceType;
            bool mentionsDbContext = serviceType.IsGenericType && Array.IndexOf(serviceType.GenericTypeArguments, typeof(OperationsDbContext)) >= 0;

            if (mentionsDbContext)
            {
                services.RemoveAt(index);
            }
        }
    }

    private void ConfigureSqlite(DbContextOptionsBuilder options)
    {
        options.UseSqlite(_connection);
    }

    private HttpMessageHandler CreateWeatherStub() => WeatherService;

    /// <summary>Builds the tables and the seed from the EF model once, the same way the real database gets them.</summary>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        using IServiceScope scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<OperationsDbContext>().Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
