using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Models;

public record OperationDto(
    int Id,
    string Key,
    string Name,
    string? Description,
    OperationKind Kind,
    InputType InputType,
    string Definition,
    bool IsActive);
