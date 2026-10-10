using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Abstractions;

/// <summary>Data access for the history of calculations.</summary>
public interface IOperationLogRepository
{
    Task AddAsync(OperationLog log, CancellationToken cancellationToken);

    /// <summary>The latest runs of one operation, newest first.</summary>
    Task<IReadOnlyList<OperationLog>> LastAsync(int operationId, int take, CancellationToken cancellationToken);

    Task<int> CountSinceAsync(int operationId, DateTime fromUtc, CancellationToken cancellationToken);
}
