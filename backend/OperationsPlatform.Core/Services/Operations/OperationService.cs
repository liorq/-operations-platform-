using FluentValidation;
using OperationsPlatform.Core.Abstractions;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Core.Services.Resolving;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Core.Services.Operations;

/// <summary>
/// Add, change and remove operations.
/// This is what lets the platform grow with no new development.
/// </summary>
public class OperationService : IOperationService
{
    private readonly IOperationRepository _operations;
    private readonly IExecutorResolver _executors;
    private readonly IValidator<OperationDto> _validator;

    public OperationService(IOperationRepository operations, IExecutorResolver executors, IValidator<OperationDto> validator)
    {
        _operations = operations;
        _executors = executors;
        _validator = validator;
    }

    public async Task<IReadOnlyList<OperationDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        IReadOnlyList<Operation> operations = await _operations.ListAsync(includeInactive, cancellationToken);

        return operations.Select(ToDto).ToList();
    }

    public async Task<OperationDto> CreateAsync(OperationDto dto, CancellationToken cancellationToken)
    {
        Operation operation = new Operation();
        await ApplyValidatedAsync(operation, dto, cancellationToken);

        await EnsureKeyIsFreeAsync(operation.Key, null, cancellationToken);

        _operations.Add(operation);
        await _operations.SaveChangesAsync(cancellationToken);

        return ToDto(operation);
    }

    public async Task<OperationDto> UpdateAsync(int id, OperationDto dto, CancellationToken cancellationToken)
    {
        Operation operation = await GetOrThrowAsync(id, cancellationToken);
        string originalKey = operation.Key;
        await ApplyValidatedAsync(operation, dto, cancellationToken);

        if (operation.Key != originalKey)
        {
            await EnsureKeyIsFreeAsync(operation.Key, id, cancellationToken);
        }

        await _operations.SaveChangesAsync(cancellationToken);

        return ToDto(operation);
    }

    /// <summary>Deactivates the operation instead of deleting it, so the history of its past runs is kept.</summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Operation operation = await GetOrThrowAsync(id, cancellationToken);
        operation.IsActive = false;

        await _operations.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureKeyIsFreeAsync(string key, int? exceptId, CancellationToken cancellationToken)
    {
        if (await _operations.KeyExistsAsync(key, exceptId, cancellationToken))
        {
            throw new ConflictException($"The key '{key}' already exists");
        }
    }

    private async Task<Operation> GetOrThrowAsync(int id, CancellationToken cancellationToken) =>
        await _operations.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Operation not found");

    /// <summary>Validates the dto with FluentValidation, copies it onto the entity, then asks the executor of its kind to check the definition.</summary>
    private async Task ApplyValidatedAsync(Operation operation, OperationDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        operation.Key = dto.Key;
        operation.Name = dto.Name.Trim();
        operation.Description = dto.Description?.Trim();
        operation.Kind = dto.Kind;
        operation.InputType = dto.InputType;
        operation.Definition = dto.Definition.Trim();
        operation.IsActive = dto.IsActive;

        _executors.Get(operation.Kind).ValidateDefinition(operation);
    }

    private static OperationDto ToDto(Operation operation) =>
        new OperationDto(
            operation.Id,
            operation.Key,
            operation.Name,
            operation.Description,
            operation.Kind,
            operation.InputType,
            operation.Definition,
            operation.IsActive);
}
