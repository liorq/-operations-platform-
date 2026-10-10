namespace OperationsPlatform.Core.Models;

/// <param name="LastThree">The last 3 successful runs of the same operation, newest first.</param>
/// <param name="CountThisMonth">Successful runs of this operation since midnight of the 1st of the current month (Israel time).</param>
public record CalculateResponse(
    string Result,
    string OperationName,
    long DurationMs,
    IReadOnlyList<HistoryItem> LastThree,
    int CountThisMonth);
