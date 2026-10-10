namespace OperationsPlatform.Domain.Enums;

/// <summary>How an operation is executed. A new kind needs one new IOperationExecutor class.</summary>
public enum OperationKind
{
    Expression = 1,
    Text = 2,
    Http = 3
}
