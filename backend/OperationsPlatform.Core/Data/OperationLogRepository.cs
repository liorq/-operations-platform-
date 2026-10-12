using OperationsPlatform.Core.Abstractions;
using OperationsPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Core.Data;

public class OperationLogRepository : IOperationLogRepository
{
    private readonly OperationsDbContext _db;

    public OperationLogRepository(OperationsDbContext db)
    {
        _db = db;
    }

    public async Task AddAndSaveAsync(OperationLog log, CancellationToken cancellationToken)
    {
        _db.OperationLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OperationLog>> LatestAsync(int operationId, int take, CancellationToken cancellationToken) =>
        await _db.OperationLogs
            .AsNoTracking()
            .Where(log => log.OperationId == operationId && log.Succeeded)
            .OrderByDescending(log => log.CreatedAtUtc)
            .ThenByDescending(log => log.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountSinceAsync(int operationId, DateTime fromUtc, CancellationToken cancellationToken) =>
        _db.OperationLogs.CountAsync(
            log => log.OperationId == operationId && log.Succeeded && log.CreatedAtUtc >= fromUtc,
            cancellationToken);
}
