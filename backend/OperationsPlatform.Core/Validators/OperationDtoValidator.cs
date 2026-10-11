using FluentValidation;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core.Models;

namespace OperationsPlatform.Core.Validators;

/// <summary>
/// Rules for adding or changing an operation. These check the shape of the data only.
/// Whether the definition itself works is checked by the executor of that kind, because that needs real logic.
/// </summary>
public class OperationDtoValidator : AbstractValidator<OperationDto>
{
    public OperationDtoValidator()
    {
        RuleFor(operation => operation.Key)
            .NotEmpty().WithMessage("A key is required")
            .Matches($@"^[a-z0-9-]{{{InputLimits.MinKeyLength},{InputLimits.MaxKeyLength}}}\z").WithMessage($"The key: {InputLimits.MinKeyLength}-{InputLimits.MaxKeyLength} characters, lowercase English letters, digits and hyphens");

        RuleFor(operation => operation.Name)
            .NotEmpty().WithMessage("A name is required")
            .MaximumLength(InputLimits.MaxNameLength).WithMessage($"The name is too long (up to {InputLimits.MaxNameLength} characters)");

        RuleFor(operation => operation.Description)
            .MaximumLength(InputLimits.MaxDescriptionLength).WithMessage($"The description is too long (up to {InputLimits.MaxDescriptionLength} characters)");

        RuleFor(operation => operation.Definition)
            .NotEmpty().WithMessage("A definition is required")
            .MaximumLength(InputLimits.MaxDefinitionLength).WithMessage($"The definition is too long (up to {InputLimits.MaxDefinitionLength} characters)");

        RuleFor(operation => operation.Kind)
            .IsInEnum().WithMessage("Invalid operation kind");

        RuleFor(operation => operation.InputType)
            .IsInEnum().WithMessage("Invalid input type");
    }
}
