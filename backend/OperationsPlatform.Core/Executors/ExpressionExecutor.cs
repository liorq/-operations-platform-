using System.Globalization;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;
using OperationsPlatform.Common.Exceptions;
using NCalc;

namespace OperationsPlatform.Core.Executors;

public class ExpressionExecutor : IOperationExecutor
{
    public OperationKind Kind => OperationKind.Expression;

    public void ValidateDefinition(Operation operation)
    {
        Expression expression = new Expression(operation.Definition);

        if (expression.HasErrors())
        {
            throw new UserInputException($"Invalid formula: {expression.Error?.Message}");
        }

        expression.Parameters[OperationParameters.NameA] = 1d;
        expression.Parameters[OperationParameters.NameB] = 2d;

        expression.Evaluate();
    }

    public Task<string> ExecuteAsync(Operation operation, string a, string b, CancellationToken cancellationToken)
    {
        Expression expression = new Expression(operation.Definition);
        expression.Parameters[OperationParameters.NameA] = ParseNumber(a, OperationParameters.LabelA);
        expression.Parameters[OperationParameters.NameB] = ParseNumber(b, OperationParameters.LabelB);

        object? value = expression.Evaluate();

        if (value is double number && !double.IsFinite(number))
        {
            throw new UserInputException("The result is undefined (for example, division by zero)");
        }

        return Task.FromResult(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
    }

    private static double ParseNumber(string text, string fieldName)
    {
        if (!NumberParser.TryParse(text, out double number))
        {
            throw new UserInputException($"Field {fieldName} must be a number");
        }

        return number;
    }
}
