using FluentValidation;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Common.Exceptions;
using OperationsPlatform.Tests.Support;
using Xunit;

namespace OperationsPlatform.Tests;

public sealed class CalculationTests : IDisposable
{
    private readonly TestApp _app = new TestApp();

    public void Dispose()
    {
        _app.Dispose();
    }

    [Theory]
    [InlineData("add", "2", "3", "5")]
    [InlineData("multiply", "4", "2.5", "10")]
    [InlineData("hypotenuse", "3", "4", "5")]
    [InlineData("levenshtein", "kitten", "sitting", "3")]
    [InlineData("concat", "שלום ", "עולם", "שלום עולם")]
    [InlineData("anagram", "Listen", "Silent", "כן, אנגרמה")]
    public async Task Calculates(string operation, string a, string b, string expected)
    {
        CalculateResponse response = await _app.Calculation.CalculateAsync(new CalculateRequest(a, b, operation), default);

        Assert.Equal(expected, response.Result);
    }

    [Fact]
    public async Task DivideByZero_IsAUserError_AndNothingIsSaved()
    {
        await Assert.ThrowsAsync<UserInputException>(() => _app.Calculation.CalculateAsync(new CalculateRequest("1", "0", "divide"), default));

        Assert.Empty(_app.Db.OperationLogs);
    }

    [Theory]
    [InlineData("x", "1", "add")]
    [InlineData("1", "1", "no-such-operation")]
    public async Task BadInput_IsRejected(string a, string b, string operation)
    {
        await Assert.ThrowsAsync<UserInputException>(() => _app.Calculation.CalculateAsync(new CalculateRequest(a, b, operation), default));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AMissingOperation_FailsValidationBeforeAnythingRuns(string operation)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", operation), default));

        Assert.Empty(_app.Db.OperationLogs);
    }

    [Fact]
    public async Task History_HasTheLastThreeRuns_NewestFirst_AndTheMonthCount()
    {
        for (int number = 1; number <= 5; number++)
        {
            _app.Clock.Now = _app.Clock.Now.AddMinutes(1);
            await _app.Calculation.CalculateAsync(new CalculateRequest(number.ToString(), "1", "add"), default);
        }

        _app.Clock.Now = _app.Clock.Now.AddMinutes(1);
        CalculateResponse response = await _app.Calculation.CalculateAsync(new CalculateRequest("100", "1", "add"), default);

        string[] lastResults = response.LastThree.Select(item => item.Result).ToArray();
        Assert.Equal(new[] { "101", "6", "5" }, lastResults);
        Assert.Equal(6, response.CountThisMonth);
    }

    [Fact]
    public async Task TheMonthStartsAtMidnightOfThe1stInIsrael_AndIgnoresOtherOperationsAndFailedAttempts()
    {
        // In October 2026 Israel is UTC+3, so 20:59 UTC on 30 September is still September there, and 21:00 UTC is already October.
        _app.Clock.Now = new DateTimeOffset(2026, 9, 30, 20, 59, 0, TimeSpan.Zero);
        await _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", "add"), default);

        _app.Clock.Now = new DateTimeOffset(2026, 9, 30, 21, 0, 0, TimeSpan.Zero);
        await _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", "add"), default);

        _app.Clock.Now = new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero);
        await _app.Calculation.CalculateAsync(new CalculateRequest("1", "1", "subtract"), default);

        await Assert.ThrowsAsync<UserInputException>(() => _app.Calculation.CalculateAsync(new CalculateRequest("x", "1", "add"), default));

        CalculateResponse response = await _app.Calculation.CalculateAsync(new CalculateRequest("2", "2", "add"), default);

        Assert.Equal(2, response.CountThisMonth);
    }
}
