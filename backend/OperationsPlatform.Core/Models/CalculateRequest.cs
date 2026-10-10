namespace OperationsPlatform.Core.Models;

/// <summary>The body of POST /api/calculate: field A, field B and the key of the operation.</summary>
public record CalculateRequest(string? A, string? B, string? Operation);
