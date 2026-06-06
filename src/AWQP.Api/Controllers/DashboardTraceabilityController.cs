using AWQP.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/dashboards")]
[Authorize]
public sealed class DashboardsController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("production")]
    public Task<ProductionDashboardDto> Production(CancellationToken cancellationToken) => dashboardService.GetProductionDashboardAsync(cancellationToken);

    [HttpGet("quality")]
    public Task<QualityDashboardDto> Quality(CancellationToken cancellationToken) => dashboardService.GetQualityDashboardAsync(cancellationToken);

    [HttpGet("inventory")]
    public Task<InventoryDashboardDto> Inventory(CancellationToken cancellationToken) => dashboardService.GetInventoryDashboardAsync(cancellationToken);

    [HttpGet("clean-room")]
    public Task<CleanRoomDashboardDto> CleanRoom(CancellationToken cancellationToken) => dashboardService.GetCleanRoomDashboardAsync(cancellationToken);

    [HttpGet("shipping")]
    public Task<ShippingDashboardDto> Shipping(CancellationToken cancellationToken) => dashboardService.GetShippingDashboardAsync(cancellationToken);
}

[ApiController]
[Route("api/traceability")]
[Authorize]
public sealed class TraceabilityController(ITraceabilityService traceabilityService) : ControllerBase
{
    [HttpGet("{serialNumber}")]
    public async Task<ActionResult<GenealogyDto>> Get(string serialNumber, CancellationToken cancellationToken)
    {
        var genealogy = await traceabilityService.GetGenealogyAsync(serialNumber, cancellationToken);
        return genealogy is null ? NotFound() : Ok(genealogy);
    }
}

[ApiController]
[Route("api/codes")]
[Authorize]
public sealed class CodesController(IBarcodeService barcodeService) : ControllerBase
{
    [HttpGet("barcode/{value}")]
    public ContentResult Barcode(string value)
    {
        return Content(barcodeService.GenerateCode128Svg(value), "image/svg+xml");
    }

    [HttpGet("qr/{value}")]
    public ContentResult QrCode(string value)
    {
        return Content(barcodeService.GenerateQrCodeSvg(value), "image/svg+xml");
    }
}
