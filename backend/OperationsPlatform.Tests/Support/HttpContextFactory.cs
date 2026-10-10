using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OperationsPlatform.Tests.Support;

/// <summary>Builds fake requests and configuration for the middleware tests.</summary>
public static class HttpContextFactory
{
    public static DefaultHttpContext Create(string method, string path)
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        return context;
    }

    public static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        Dictionary<string, string?> data = values.ToDictionary(value => value.Key, value => (string?)value.Value);

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }
}
