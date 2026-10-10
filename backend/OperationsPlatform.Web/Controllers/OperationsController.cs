using OperationsPlatform.Core.Services.Operations;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Web.Routes;
using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Controllers;

/// <summary>
/// The list of operations, and adding, changing and removing them.
/// Everything except the list requires the X-Admin-Key header (checked by AdminKeyMiddleware).
/// </summary>
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

    /// <summary>Deactivates the operation. Its history stays in the database.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _operations.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
