namespace OperationsPlatform.Core.Models;

public record CalculateResponse(
    string Result,
    string OperationName,
    long DurationMs,
    IReadOnlyList<HistoryItem> LastThree,
    int CountThisMonth);
