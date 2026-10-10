using OperationsPlatform.Core.Executors;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Services.Resolving;

/// <summary>Finds the executor that handles a given kind of operation.</summary>
public interface IExecutorResolver
{
    IOperationExecutor Get(OperationKind kind);
}
