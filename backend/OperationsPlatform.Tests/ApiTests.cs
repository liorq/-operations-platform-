using System.Net;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace OperationsPlatform.Tests;

/// <summary>Full-stack tests: a real HTTP request goes through every middleware, controller, service and the database.</summary>
public class ApiTests : IClassFixture<PlatformWebApplicationFactory>
{
    private readonly PlatformWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiTests(PlatformWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ReturnsTheResultTheLastRunsAndTheMonthCount()
    {
        CalculateRequest request = new CalculateRequest("2", "10", "power");

        await TestJson.PostAsync(_client, "/api/calculate", request);
        HttpResponseMessage response = await TestJson.PostAsync(_client, "/api/calculate", request);

        CalculateResponse body = await TestJson.ReadAsync<CalculateResponse>(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1024", body.Result);
        Assert.Equal(2, body.CountThisMonth);
        Assert.Equal(2, body.LastThree.Count);
    }

    [Fact]
    public async Task Calculate_DivideByZero_Returns400WithAReadableMessage()
    {
        HttpResponseMessage response = await TestJson.PostAsync(_client, "/api/calculate", new CalculateRequest("1", "0", "divide"));

        ProblemDetails problem = await TestJson.ReadAsync<ProblemDetails>(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("division by zero", problem.Title);
    }

    [Fact]
    public async Task CreateOperation_WithoutTheAdminKey_Returns401()
    {
        OperationDto dto = new OperationDto(0, "no-key", "No key", null, OperationKind.Expression, InputType.Number, "a + b", true);

        HttpResponseMessage response = await TestJson.PostAsync(_client, "/api/operations", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WeatherOperation_CallsTheExternalService()
    {
        HttpResponseMessage response = await TestJson.PostAsync(_client, "/api/calculate", new CalculateRequest("32.08", "34.78", "weather"));

        CalculateResponse body = await TestJson.ReadAsync<CalculateResponse>(response);
        Assert.Equal("23.5", body.Result);
        Assert.Contains(_factory.WeatherService.RequestedUrls, url => url.Contains("latitude=32.08&longitude=34.78"));
    }

    [Fact]
    public async Task WeatherOperation_WithAnImpossibleLatitude_Returns400()
    {
        HttpResponseMessage response = await TestJson.PostAsync(_client, "/api/calculate", new CalculateRequest("999", "34.78", "weather"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
