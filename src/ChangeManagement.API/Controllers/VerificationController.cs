using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.API.Controllers;

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;
    private readonly IValidator<VerificationActionRequest> _validator;

    public VerificationController(IVerificationService verificationService, IValidator<VerificationActionRequest> validator)
    {
        _verificationService = verificationService;
        _validator = validator;
    }

    [HttpGet("safety")]
    [Authorize(Roles = $"{AppRoles.Safety},{AppRoles.Administrator}")]
    public async Task<IActionResult> SafetyQueue([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _verificationService.GetAssignedRequestsAsync(VerificationStage.Safety, query, cancellationToken));

    [HttpGet("department-head")]
    [Authorize(Roles = $"{AppRoles.DepartmentHead},{AppRoles.Administrator}")]
    public async Task<IActionResult> DeptHeadQueue([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _verificationService.GetAssignedRequestsAsync(VerificationStage.DepartmentHead, query, cancellationToken));

    [HttpGet("qa")]
    [Authorize(Roles = $"{AppRoles.QA},{AppRoles.Administrator}")]
    public async Task<IActionResult> QaQueue([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _verificationService.GetAssignedRequestsAsync(VerificationStage.QA, query, cancellationToken));

    [HttpPost("{erId:guid}/safety")]
    [Authorize(Roles = $"{AppRoles.Safety},{AppRoles.Administrator}")]
    public async Task<IActionResult> SafetyAct(Guid erId, [FromBody] VerificationActionRequest request, CancellationToken cancellationToken) =>
        await Act(erId, VerificationStage.Safety, request, cancellationToken);

    [HttpPost("{erId:guid}/department-head")]
    [Authorize(Roles = $"{AppRoles.DepartmentHead},{AppRoles.Administrator}")]
    public async Task<IActionResult> DeptHeadAct(Guid erId, [FromBody] VerificationActionRequest request, CancellationToken cancellationToken) =>
        await Act(erId, VerificationStage.DepartmentHead, request, cancellationToken);

    [HttpPost("{erId:guid}/qa")]
    [Authorize(Roles = $"{AppRoles.QA},{AppRoles.Administrator}")]
    public async Task<IActionResult> QaAct(Guid erId, [FromBody] VerificationActionRequest request, CancellationToken cancellationToken) =>
        await Act(erId, VerificationStage.QA, request, cancellationToken);

    [HttpPost("{erId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid erId, [FromBody] AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.AddCommentAsync(erId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private async Task<IActionResult> Act(Guid erId, VerificationStage stage, VerificationActionRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { success = false, errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _verificationService.ActAsync(erId, stage, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
