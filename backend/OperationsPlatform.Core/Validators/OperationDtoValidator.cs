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
            .NotEmpty().WithMessage("חובה מפתח")
            .Matches($@"^[a-z0-9-]{{{InputLimits.MinKeyLength},{InputLimits.MaxKeyLength}}}\z").WithMessage($"המפתח: {InputLimits.MinKeyLength}-{InputLimits.MaxKeyLength} תווים, אותיות קטנות באנגלית, ספרות ומקף");

        RuleFor(operation => operation.Name)
            .NotEmpty().WithMessage("חובה שם לפעולה")
            .MaximumLength(InputLimits.MaxNameLength).WithMessage($"השם ארוך מדי (עד {InputLimits.MaxNameLength} תווים)");

        RuleFor(operation => operation.Description)
            .MaximumLength(InputLimits.MaxDescriptionLength).WithMessage($"התיאור ארוך מדי (עד {InputLimits.MaxDescriptionLength} תווים)");

        RuleFor(operation => operation.Definition)
            .NotEmpty().WithMessage("חובה הגדרה (Definition)")
            .MaximumLength(InputLimits.MaxDefinitionLength).WithMessage($"ההגדרה ארוכה מדי (עד {InputLimits.MaxDefinitionLength} תווים)");

        RuleFor(operation => operation.Kind)
            .IsInEnum().WithMessage("סוג פעולה לא חוקי");

        RuleFor(operation => operation.InputType)
            .IsInEnum().WithMessage("סוג קלט לא חוקי");
    }
}
