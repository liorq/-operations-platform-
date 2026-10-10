using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using OperationsPlatform.Common.Exceptions;
using OperationsPlatform.Tests.Support;
using OperationsPlatform.Web.Handlers;
using Xunit;

namespace OperationsPlatform.Tests;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

    private static Exception CreateException(string kind) => kind switch
    {
        "validation" => new ValidationException(new[] { new ValidationFailure("key", "bad key") }),
        "json" => new JsonException("bad definition"),
        "user" => new UserInputException("bad input"),
        _ => new InvalidOperationException("boom"),
    };

    [Theory]
    [InlineData("validation", 400)]
    [InlineData("user", 400)]
    [InlineData("json", 400)]
    [InlineData("unexpected", 500)]
    public async Task MapsEachKindOfExceptionToAStatusCode(string kind, int expectedStatus)
    {
        DefaultHttpContext context = HttpContextFactory.Create("POST", "/api/calculate");

        bool handled = await _handler.TryHandleAsync(context, CreateException(kind), default);

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task AnUnexpectedError_DoesNotLeakDetailsToTheCaller()
    {
        DefaultHttpContext context = HttpContextFactory.Create("GET", "/api/operations");

        await _handler.TryHandleAsync(context, CreateException("unexpected"), default);

        context.Response.Body.Position = 0;
        string body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("boom", body);
    }
}
