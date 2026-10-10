using OperationsPlatform.Core.Abstractions;
using OperationsPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Core.Data;

public class OperationRepository : IOperationRepository
{
    private readonly OperationsDbContext _db;

    public OperationRepository(OperationsDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Operation>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        IQueryable<Operation> query = _db.Operations.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(operation => operation.IsActive);
        }

        return await query.OrderBy(operation => operation.Id).ToListAsync(cancellationToken);
    }

    public Task<Operation?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _db.Operations.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public Task<Operation?> GetActiveByKeyAsync(string key, CancellationToken cancellationToken) =>
        _db.Operations.FirstOrDefaultAsync(operation => operation.Key == key && operation.IsActive, cancellationToken);

    public Task<bool> KeyExistsAsync(string key, int? exceptId, CancellationToken cancellationToken) =>
        _db.Operations.AnyAsync(operation => operation.Key == key && operation.Id != exceptId, cancellationToken);

    public void Add(Operation operation) => _db.Operations.Add(operation);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync(cancellationToken);
}
