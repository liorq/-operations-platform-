using System.Diagnostics;
using FluentValidation;
using OperationsPlatform.Common.Exceptions;
using OperationsPlatform.Core.Abstractions;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Core.Services.Resolving;
using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Services.Calculation;

/// <summary>
/// The core business logic: find the operation, run it, record the run,
/// and return the result together with the history required by chapter B.
/// </summary>
public class CalculationService : ICalculationService
{
    private const int HistorySize = 3;

    private readonly IOperationRepository _operations;
    private readonly IOperationLogRepository _logs;
    private readonly IExecutorResolver _executors;
    private readonly IValidator<CalculateRequest> _validator;
    private readonly TimeProvider _clock;

    public CalculationService(
        IOperationRepository operations,
        IOperationLogRepository logs,
        IExecutorResolver executors,
        IValidator<CalculateRequest> validator,
        TimeProvider clock)
    {
        _operations = operations;
        _logs = logs;
        _executors = executors;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CalculateResponse> CalculateAsync(CalculateRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        Operation operation = await FindOperationAsync(request.Operation!, cancellationToken);
        string a = request.A ?? string.Empty;
        string b = request.B ?? string.Empty;

        Stopwatch stopwatch = Stopwatch.StartNew();
        string result = await _executors.Get(operation.Kind).ExecuteAsync(operation, a, b, cancellationToken);
        long durationMs = stopwatch.ElapsedMilliseconds;

        OperationLog log = new OperationLog
        {
            OperationId = operation.Id,
            A = a,
            B = b,
            Result = result,
            DurationMs = durationMs,
            CreatedAtUtc = _clock.GetUtcNow().UtcDateTime
        };
        await _logs.AddAsync(log, cancellationToken);

        IReadOnlyList<OperationLog> lastRuns = await _logs.LastAsync(operation.Id, HistorySize, cancellationToken);
        int countThisMonth = await _logs.CountSinceAsync(operation.Id, StartOfMonthUtc(), cancellationToken);

        return new CalculateResponse(result, operation.Name, durationMs, ToHistoryItems(lastRuns), countThisMonth);
    }

    private async Task<Operation> FindOperationAsync(string key, CancellationToken cancellationToken) =>
        await _operations.GetActiveByKeyAsync(key, cancellationToken) ?? throw new UserInputException($"הפעולה '{key}' לא קיימת");

    private static IReadOnlyList<HistoryItem> ToHistoryItems(IReadOnlyList<OperationLog> logs) => logs.Select(ToHistoryItem).ToList();

    private static HistoryItem ToHistoryItem(OperationLog log) => new HistoryItem(log.A, log.B, log.Result, log.DurationMs, log.CreatedAtUtc);

    /// <summary>Midnight of the 1st of this month in the clock's time zone (Israel), as a UTC time for the database query.</summary>
    private DateTime StartOfMonthUtc()
    {
        DateTime localNow = _clock.GetLocalNow().DateTime;
        DateTime firstOfMonth = new DateTime(localNow.Year, localNow.Month, 1);

        return TimeZoneInfo.ConvertTimeToUtc(firstOfMonth, _clock.LocalTimeZone);
    }
}
