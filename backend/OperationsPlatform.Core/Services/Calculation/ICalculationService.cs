using OperationsPlatform.Core.Models;

namespace OperationsPlatform.Core.Services.Calculation;

public interface ICalculationService
{
    Task<CalculateResponse> CalculateAsync(CalculateRequest request, CancellationToken cancellationToken);
}
