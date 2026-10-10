using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Domain.Entities;

/// <summary>
/// An operation lives in the database. A formula (Expression), a call to an allowed external API (Http) or a built-in text function (Text)
/// can be added, changed or removed with no new development. A new text function or a new kind of operation needs code.
/// </summary>
public class Operation
{
    public int Id { get; set; }

    /// <summary>Stable identifier used by the API, for example "add".</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Name shown to users.</summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OperationKind Kind { get; set; }

    public InputType InputType { get; set; }

    /// <summary>
    /// Expression: a formula over a and b. Text: the name of a text function.
    /// Http: JSON with url and path.
    /// </summary>
    public string Definition { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
