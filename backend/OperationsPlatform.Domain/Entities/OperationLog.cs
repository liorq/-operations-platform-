namespace OperationsPlatform.Domain.Entities;

/// <summary>One row for every successful calculation. This is the history required by chapter B.</summary>
public class OperationLog
{
    public long Id { get; set; }

    public int OperationId { get; set; }

    public Operation? Operation { get; set; }

    public string A { get; set; } = string.Empty;

    public string B { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public long DurationMs { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
