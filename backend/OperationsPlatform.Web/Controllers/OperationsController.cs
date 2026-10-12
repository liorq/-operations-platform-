using OperationsPlatform.Core.Services.Operations;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Web.Routes;
using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Controllers;

[ApiController]
[Route(ApiRoutes.Operations)]
public class OperationsController : ControllerBase
{
    private readonly IOperationService _operations;

    public OperationsController(IOperationService operations)
    {
        _operations = operations;
    }

    [HttpGet]
    public Task<IReadOnlyList<OperationDto>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken) =>
        _operations.ListAsync(includeInactive, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<OperationDto>> Create(
        [FromBody] OperationDto dto,
        CancellationToken cancellationToken)
    {
        OperationDto created = await _operations.CreateAsync(dto, cancellationToken);

        return Created($"{ApiRoutes.Operations}/{created.Id}", created);
    }

    [HttpPut("{id:int}")]
    public Task<OperationDto> Update(int id, [FromBody] OperationDto dto, CancellationToken cancellationToken) =>
        _operations.UpdateAsync(id, dto, cancellationToken);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _operations.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
