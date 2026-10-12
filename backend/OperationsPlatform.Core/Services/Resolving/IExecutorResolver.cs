using OperationsPlatform.Core.Executors;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Services.Resolving;

public interface IExecutorResolver
{
    IOperationExecutor Get(OperationKind kind);
}
