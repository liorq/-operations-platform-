using OperationsPlatform.Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace OperationsPlatform.Tests.Support;

/// <summary>
/// Starts the whole application in memory: real controllers, real middleware, real services, and a real SQL Server database
/// that the application creates by itself with its migration. Only the weather service is replaced by a stub.
/// </summary>
public class PlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminKey = "test-admin-key";

    private const string WeatherJson = "{\"current\":{\"temperature_2m\":23.5}}";

    private readonly TestDatabase _database = new TestDatabase();

    public StubHttpMessageHandler WeatherService { get; } = new StubHttpMessageHandler(WeatherJson);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"ConnectionStrings:{ConfigKeys.ConnectionStringName}", _database.ConnectionString);
        builder.UseSetting(ConfigKeys.AdminKey, AdminKey);

        builder.ConfigureTestServices(ReplaceWeatherService);
    }

    private void ReplaceWeatherService(IServiceCollection services) =>
        services.AddHttpClient(ExternalApi.ClientName).ConfigurePrimaryHttpMessageHandler(CreateWeatherStub);

    private HttpMessageHandler CreateWeatherStub() => WeatherService;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _database.Dispose();
        }
    }
}
