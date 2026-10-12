using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Abstractions;

public interface IOperationLogRepository
{
    Task AddAndSaveAsync(OperationLog log, CancellationToken cancellationToken);

    Task<IReadOnlyList<OperationLog>> LatestAsync(int operationId, int take, CancellationToken cancellationToken);

    Task<int> CountSinceAsync(int operationId, DateTime fromUtc, CancellationToken cancellationToken);
}
