using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Executors;

public interface IOperationExecutor
{
    OperationKind Kind { get; }

    void ValidateDefinition(Operation operation);

    Task<string> ExecuteAsync(Operation operation, string a, string b, CancellationToken cancellationToken);
}
