using OperationsPlatform.Core.Services.Calculation;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Web.Routes;
using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Controllers;

/// <summary>Runs an operation on field A and field B.</summary>
[ApiController]
[Route(ApiRoutes.Calculate)]
public class CalculateController : ControllerBase
{
    private readonly ICalculationService _calculation;

    public CalculateController(ICalculationService calculation)
    {
        _calculation = calculation;
    }

    /// <summary>Returns the result, the last 3 runs of the same operation, and how many ran this month.</summary>
    [HttpPost]
    public Task<CalculateResponse> Calculate([FromBody] CalculateRequest request, CancellationToken cancellationToken) =>
        _calculation.CalculateAsync(request, cancellationToken);
}
