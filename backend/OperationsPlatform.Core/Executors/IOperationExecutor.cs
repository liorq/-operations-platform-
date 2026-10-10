using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Executors;

/// <summary>Runs operations of one kind. A new kind needs a new value in OperationKind, an implementation of this interface, and its registration in CoreServiceExtensions.</summary>
public interface IOperationExecutor
{
    OperationKind Kind { get; }

    /// <summary>Checks that the definition is valid. Called whenever an operation is saved or tested.</summary>
    void ValidateDefinition(Operation operation);

    Task<string> ExecuteAsync(Operation operation, string a, string b, CancellationToken cancellationToken);
}
