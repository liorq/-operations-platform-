using FluentValidation;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core.Models;

namespace OperationsPlatform.Core.Validators;

/// <summary>Rules for the body of POST /api/calculate.</summary>
public class CalculateRequestValidator : AbstractValidator<CalculateRequest>
{
    public CalculateRequestValidator()
    {
        RuleFor(request => request.Operation)
            .NotEmpty().WithMessage("חובה לבחור פעולה")
            .MaximumLength(InputLimits.MaxKeyLength).WithMessage("מפתח הפעולה ארוך מדי");

        RuleFor(request => request.A)
            .MaximumLength(InputLimits.MaxInputLength).WithMessage($"שדה א' ארוך מדי (עד {InputLimits.MaxInputLength} תווים)");

        RuleFor(request => request.B)
            .MaximumLength(InputLimits.MaxInputLength).WithMessage($"שדה ב' ארוך מדי (עד {InputLimits.MaxInputLength} תווים)");
    }
}
