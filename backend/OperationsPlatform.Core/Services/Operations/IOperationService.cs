using OperationsPlatform.Core.Models;

namespace OperationsPlatform.Core.Services.Operations;

/// <summary>Add, change and remove operations.</summary>
public interface IOperationService
{
    Task<IReadOnlyList<OperationDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<OperationDto> CreateAsync(OperationDto dto, CancellationToken cancellationToken);

    Task<OperationDto> UpdateAsync(int id, OperationDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
