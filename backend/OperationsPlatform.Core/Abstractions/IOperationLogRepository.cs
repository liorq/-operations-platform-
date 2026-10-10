using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Abstractions;

/// <summary>Data access for the history of calculations.</summary>
public interface IOperationLogRepository
{
    /// <summary>Adds the run and saves it.</summary>
    Task AddAndSaveAsync(OperationLog log, CancellationToken cancellationToken);

    /// <summary>The latest runs of one operation, newest first.</summary>
    Task<IReadOnlyList<OperationLog>> LatestAsync(int operationId, int take, CancellationToken cancellationToken);

    Task<int> CountSinceAsync(int operationId, DateTime fromUtc, CancellationToken cancellationToken);
}
