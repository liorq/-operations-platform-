namespace OperationsPlatform.Core.Models;

public record HistoryItem(string A, string B, string Result, long DurationMs, DateTime CreatedAtUtc);
