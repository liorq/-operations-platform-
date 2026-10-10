using System.Net;
using Microsoft.AspNetCore.Mvc;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Tests.Support;
using Xunit;

namespace OperationsPlatform.Tests;

/// <summary>The input validation middleware and the validators, tested through real HTTP requests.</summary>
public class InputValidationApiTests : IClassFixture<PlatformWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InputValidationApiTests(PlatformWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("{\"a\":\"<script>alert(1)</script>\",\"b\":\"2\",\"operation\":\"add\"}")]
    [InlineData("{\"a\":\"1\",\"b\":\"2\",\"operation\":\"add\",\"extra\":{\"list\":[\"<img src=x>\"]}}")]
    [InlineData("{\"a<b\":\"1\",\"b\":\"2\",\"operation\":\"add\"}")]
    public async Task ForbiddenCharacters_Return400WithNoDetails(string json)
    {
        HttpResponseMessage response = await TestJson.PostRawAsync(_client, "/api/calculate", json);

        string body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("errors", body);
        Assert.DoesNotContain("script", body);
    }

    [Fact]
    public async Task InvalidOperation_ReportsEveryFieldAtOnce_NamedLikeTheJson()
    {
        string json = "{\"key\":\"Bad Key\",\"name\":\"\",\"kind\":\"Expression\",\"inputType\":\"Number\",\"definition\":\"\",\"isActive\":true}";

        HttpResponseMessage response = await TestJson.PostRawAsync(_client, "/api/operations", json, PlatformWebApplicationFactory.AdminKey);

        ValidationProblemDetails problem = await TestJson.ReadAsync<ValidationProblemDetails>(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.Errors.ContainsKey("key"));
        Assert.True(problem.Errors.ContainsKey("name"));
        Assert.True(problem.Errors.ContainsKey("definition"));
    }

    [Fact]
    public async Task MissingOperation_IsReportedOnTheOperationField()
    {
        HttpResponseMessage response = await TestJson.PostRawAsync(_client, "/api/calculate", "{\"a\":\"1\",\"b\":\"2\"}");

        ValidationProblemDetails problem = await TestJson.ReadAsync<ValidationProblemDetails>(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.Errors.ContainsKey("operation"));
    }

    [Theory]
    [InlineData("Sqrt(a*a + b*b)")]
    [InlineData("Max(a, b) + Abs(a - b) * 2 / 3 % 4")]
    [InlineData("שלום עולם")]
    [InlineData("אלכסון (פיתגורס)")]
    [InlineData("O'Brien & Sons, Ltd.")]
    [InlineData("{\"url\":\"https://api.open-meteo.com/v1?latitude={a}\",\"path\":\"x\"}")]
    [InlineData("line one\r\nline two\ttabbed")]
    [InlineData("price: $5 ~ 6 | 7 ^ 2 = 1")]
    [InlineData("שלום\u200f")]
    public async Task NormalText_PassesTheAllowList(string text)
    {
        HttpResponseMessage response = await PostFieldAsync(text);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("<SCRIPT SRC=//evil.example/x.js></SCRIPT>")]
    [InlineData("<svg/onload=alert(1)>")]
    [InlineData("a > b")]
    [InlineData("if(a<b, 1, 0)")]
    [InlineData("hello\u0000world")]
    [InlineData("bell\u0007")]
    [InlineData("name\u202Egnp.exe")]
    [InlineData("vertical\u000Btab")]
    [InlineData("hello \U0001F600")]
    [InlineData("back`tick")]
    public async Task MarkupControlCharactersAndOtherSymbols_AreRefused(string text)
    {
        HttpResponseMessage response = await PostFieldAsync(text);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostFieldAsync(string text) =>
        TestJson.PostAsync(_client, "/api/calculate", new CalculateRequest(text, "x", "concat"));
}
