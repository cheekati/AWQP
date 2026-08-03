using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/mes")]
[Authorize(Policy = "Production")]
public sealed class ManufacturingController : ControllerBase
{
    private readonly IManufacturingService _manufacturingService;
    public ManufacturingController(IManufacturingService manufacturingService) => _manufacturingService = manufacturingService;

    [HttpPost("work-orders")]
    public async Task<IActionResult> CreateWorkOrder(CreateWorkOrderRequest request, CancellationToken cancellationToken) => Ok(await _manufacturingService.CreateWorkOrderAsync(request, cancellationToken));

    [HttpPost("operations/start")]
    public async Task<IActionResult> Start(OperationStartRequest request, CancellationToken cancellationToken)
    {
        await _manufacturingService.StartOperationAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("operations/complete")]
    public async Task<IActionResult> Complete(OperationCompleteRequest request, CancellationToken cancellationToken)
    {
        await _manufacturingService.CompleteOperationAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("traceability/{serialNumber}")]
    [Authorize]
    public async Task<IActionResult> Traceability(string serialNumber, CancellationToken cancellationToken)
    {
        var result = await _manufacturingService.GetGenealogyAsync(serialNumber, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
