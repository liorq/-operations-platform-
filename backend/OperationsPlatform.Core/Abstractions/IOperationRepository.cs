using OperationsPlatform.Domain.Entities;

namespace OperationsPlatform.Core.Abstractions;

/// <summary>Data access for operations. The Application layer knows only this interface, not EF Core.</summary>
public interface IOperationRepository
{
    Task<IReadOnlyList<Operation>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<Operation?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Operation?> GetActiveByKeyAsync(string key, CancellationToken cancellationToken);

    Task<bool> KeyExistsAsync(string key, int? exceptId, CancellationToken cancellationToken);

    void Add(Operation operation);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
