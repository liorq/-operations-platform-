using OperationsPlatform.Core.Models;

namespace OperationsPlatform.Core.Services.Calculation;

/// <summary>Runs an operation and reports its history. Controllers depend on this contract, not on the class.</summary>
public interface ICalculationService
{
    Task<CalculateResponse> CalculateAsync(CalculateRequest request, CancellationToken cancellationToken);
}
