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
            .NotEmpty().WithMessage("An operation is required")
            .MaximumLength(InputLimits.MaxKeyLength).WithMessage("The operation key is too long");

        RuleFor(request => request.A)
            .MaximumLength(InputLimits.MaxInputLength).WithMessage($"Field A is too long (up to {InputLimits.MaxInputLength} characters)");

        RuleFor(request => request.B)
            .MaximumLength(InputLimits.MaxInputLength).WithMessage($"Field B is too long (up to {InputLimits.MaxInputLength} characters)");
    }
}
