using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.API.Controllers;

[ApiController]
[Route("api/coo")]
[Authorize(Roles = $"{AppRoles.COO},{AppRoles.Administrator}")]
public class CooController : ControllerBase
{
    private readonly ICooApprovalService _cooService;
    private readonly IValidator<CooActionRequest> _validator;

    public CooController(ICooApprovalService cooService, IValidator<CooActionRequest> validator)
    {
        _cooService = cooService;
        _validator = validator;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _cooService.GetPendingAsync(query, cancellationToken));

    [HttpPost("{erId:guid}/action")]
    public async Task<IActionResult> Act(Guid erId, [FromBody] CooActionRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { success = false, errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _cooService.ActAsync(erId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] PagedQuery query, CancellationToken cancellationToken) =>
        Ok(await _notificationService.GetMyNotificationsAsync(query, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken) =>
        Ok(new { success = true, data = await _notificationService.GetUnreadCountAsync(cancellationToken) });

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken) =>
        Ok(await _notificationService.MarkAsReadAsync(id, cancellationToken));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken) =>
        Ok(await _notificationService.MarkAllAsReadAsync(cancellationToken));
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken cancellationToken) =>
        Ok(await _dashboardService.GetStatsAsync(cancellationToken));
}
