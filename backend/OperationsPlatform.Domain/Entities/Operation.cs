using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Domain.Entities;

public class Operation
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OperationKind Kind { get; set; }

    public InputType InputType { get; set; }

    public string Definition { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
