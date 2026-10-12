using OperationsPlatform.Core.Services.Calculation;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Web.Routes;
using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Controllers;

[ApiController]
[Route(ApiRoutes.Calculate)]
public class CalculateController : ControllerBase
{
    private readonly ICalculationService _calculation;

    public CalculateController(ICalculationService calculation)
    {
        _calculation = calculation;
    }

    [HttpPost]
    public Task<CalculateResponse> Calculate([FromBody] CalculateRequest request, CancellationToken cancellationToken) =>
        _calculation.CalculateAsync(request, cancellationToken);
}
