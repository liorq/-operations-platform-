using OperationsPlatform.Core.Models;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Common.Exceptions;
using OperationsPlatform.Tests.Support;
using Xunit;

namespace OperationsPlatform.Tests;

public sealed class OperationServiceTests : IDisposable
{
    private const string AllowedWeatherDefinition =
        "{\"url\":\"https://api.open-meteo.com/v1/forecast?latitude={a}&longitude={b}\",\"path\":\"x\"}";

    private readonly TestApp _app = new TestApp();

    public void Dispose()
    {
        _app.Dispose();
    }

    private static OperationDto CreateDto(string key = "plus10", string definition = "a + 10", OperationKind kind = OperationKind.Expression)
    {
        return new OperationDto(0, key, "Plus ten", null, kind, InputType.Number, definition, true);
    }

    [Fact]
    public async Task Update_ChangesTheBehaviour()
    {
        OperationDto created = await _app.Operations.CreateAsync(CreateDto(), default);

        await _app.Operations.UpdateAsync(created.Id, CreateDto(definition: "a + 20"), default);

        CalculateResponse response = await _app.Calculation.CalculateAsync(new CalculateRequest("5", "0", "plus10"), default);
        Assert.Equal("25", response.Result);
    }

    [Fact]
    public async Task Delete_Deactivates_ButKeepsTheHistory()
    {
        await _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", "add"), default);

        IReadOnlyList<OperationDto> all = await _app.Operations.ListAsync(true, default);
        await _app.Operations.DeleteAsync(all.Single(operation => operation.Key == "add").Id, default);

        await Assert.ThrowsAsync<UserInputException>(() => _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", "add"), default));

        IReadOnlyList<OperationDto> activeOperations = await _app.Operations.ListAsync(false, default);
        IReadOnlyList<OperationDto> allOperations = await _app.Operations.ListAsync(true, default);

        Assert.DoesNotContain(activeOperations, operation => operation.Key == "add");
        Assert.Contains(allOperations, operation => operation.Key == "add");
        Assert.Single(_app.Db.OperationLogs);
    }

    [Fact]
    public async Task DuplicateKey_IsRejected()
    {
        await Assert.ThrowsAsync<UserInputException>(() => _app.Operations.CreateAsync(CreateDto(key: "add"), default));
    }

    [Theory]
    [InlineData("a +", OperationKind.Expression)]
    [InlineData("a + c", OperationKind.Expression)]
    [InlineData("no-such-function", OperationKind.Text)]
    [InlineData("not json", OperationKind.Http)]
    public async Task ADefinitionThatDoesNotWork_IsRejected(string definition, OperationKind kind)
    {
        await Assert.ThrowsAnyAsync<Exception>(() => _app.Operations.CreateAsync(CreateDto("ok-key", definition, kind), default));
    }

    [Theory]
    [InlineData("https://evil.example.com/x?a={a}")]
    [InlineData("https://{a}.api.open-meteo.com/x")]
    [InlineData("http://api.open-meteo.com/x?a={a}")]
    [InlineData("https://127.0.0.1/admin")]
    public async Task HttpOperation_WithAForbiddenAddress_IsRejected(string url)
    {
        string definition = "{\"url\":\"" + url + "\",\"path\":\"x\"}";
        await Assert.ThrowsAsync<UserInputException>(() => _app.Operations.CreateAsync(CreateDto("web", definition, OperationKind.Http), default));
    }

    [Fact]
    public async Task HttpOperation_WithAnAllowedAddress_IsAccepted()
    {
        OperationDto dto = CreateDto("web", AllowedWeatherDefinition, OperationKind.Http);

        OperationDto created = await _app.Operations.CreateAsync(dto, default);

        Assert.Equal("web", created.Key);
    }
}
