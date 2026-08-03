using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/dashboards")]
[Authorize]
public sealed class DashboardsController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardsController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("executive")]
    public async Task<IActionResult> Executive(CancellationToken cancellationToken) => Ok(await _dashboardService.GetExecutiveDashboardAsync(cancellationToken));
}
