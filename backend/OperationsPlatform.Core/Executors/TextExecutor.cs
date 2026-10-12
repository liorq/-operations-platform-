using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Core.Executors;

public class TextExecutor : IOperationExecutor
{
    public OperationKind Kind => OperationKind.Text;

    public void ValidateDefinition(Operation operation)
    {
        if (!TextFunctions.Names.Contains(operation.Definition, StringComparer.OrdinalIgnoreCase))
        {
            string names = string.Join(", ", TextFunctions.Names);
            throw new UserInputException($"Unknown text function. Options: {names}");
        }
    }

    public Task<string> ExecuteAsync(Operation operation, string a, string b, CancellationToken cancellationToken)
        => Task.FromResult(TextFunctions.Run(operation.Definition, a, b));
}
