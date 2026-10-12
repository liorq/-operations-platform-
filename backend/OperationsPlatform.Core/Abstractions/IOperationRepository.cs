using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Abstractions;

public interface IOperationRepository
{
    Task<IReadOnlyList<Operation>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<Operation?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Operation?> GetActiveByKeyAsync(string key, CancellationToken cancellationToken);

    Task<bool> KeyExistsAsync(string key, int? exceptId, CancellationToken cancellationToken);

    void Add(Operation operation);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
