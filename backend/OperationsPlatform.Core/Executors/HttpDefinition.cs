namespace OperationsPlatform.Core.Executors;

/// <summary>
/// The JSON stored in the definition of an Http operation.
/// Url may contain {a} and {b}. Path is a dotted path to the value in the response.
/// </summary>
public record HttpDefinition(string Url, string Path);
