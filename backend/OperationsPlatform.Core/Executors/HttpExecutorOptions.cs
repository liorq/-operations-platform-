namespace OperationsPlatform.Core.Executors;

/// <summary>Which external hosts an Http operation may call. Any other host is rejected, which prevents SSRF.</summary>
public record HttpExecutorOptions(string[] AllowedHosts);
