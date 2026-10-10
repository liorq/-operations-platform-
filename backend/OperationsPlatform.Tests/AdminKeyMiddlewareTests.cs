using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Web.Middleware;
using OperationsPlatform.Tests.Support;
using Xunit;

namespace OperationsPlatform.Tests;

public class AdminKeyMiddlewareTests
{
    [Theory]
    [InlineData("POST", "/api/operations", "", StatusCodes.Status401Unauthorized)]
    [InlineData("POST", "/api/operations", "wrong-key", StatusCodes.Status401Unauthorized)]
    [InlineData("POST", "/api/operations", "secret", 200)]
    [InlineData("PUT", "/api/operations/3", "secret", 200)]
    [InlineData("DELETE", "/api/operations/3", "secret", 200)]
    [InlineData("GET", "/api/operations", null, 200)]
    [InlineData("POST", "/api/calculate", null, 200)]
    public async Task ProtectsOnlyChangesToOperations(string method, string path, string? key, int expectedStatus)
    {
        IConfiguration configuration = HttpContextFactory.CreateConfiguration((ConfigKeys.AdminKey, "secret"));
        AdminKeyMiddleware middleware = new AdminKeyMiddleware(AnswerOk, configuration);
        DefaultHttpContext context = HttpContextFactory.Create(method, path);

        if (key is not null)
        {
            context.Request.Headers["X-Admin-Key"] = key;
        }

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task WithNoKeyConfigured_EveryChangeIsRefused()
    {
        AdminKeyMiddleware middleware = new AdminKeyMiddleware(AnswerOk, HttpContextFactory.CreateConfiguration());
        DefaultHttpContext context = HttpContextFactory.Create("POST", "/api/operations");
        context.Request.Headers["X-Admin-Key"] = string.Empty;

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    /// <summary>A next-step that always answers 200, standing in for the rest of the pipeline.</summary>
    private static Task AnswerOk(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;

        return Task.CompletedTask;
    }
}
