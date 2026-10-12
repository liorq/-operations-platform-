using OperationsPlatform.Core.Executors;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Services.Resolving;

public class ExecutorResolver : IExecutorResolver
{
    private readonly IEnumerable<IOperationExecutor> _executors;

    public ExecutorResolver(IEnumerable<IOperationExecutor> executors)
    {
        _executors = executors;
    }

    public IOperationExecutor Get(OperationKind kind)
        => _executors.FirstOrDefault(executor => executor.Kind == kind)
            ?? throw new InvalidOperationException($"No executor handles the kind {kind}");
}
