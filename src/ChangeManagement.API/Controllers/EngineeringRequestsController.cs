using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.API.Controllers;

[ApiController]
[Route("api/engineering-requests")]
[Authorize]
public class EngineeringRequestsController : ControllerBase
{
    private readonly IEngineeringRequestService _service;
    private readonly IValidator<CreateEngineeringRequestDto> _createValidator;
    private readonly IValidator<UpdateEngineeringRequestDto> _updateValidator;

    public EngineeringRequestsController(
        IEngineeringRequestService service,
        IValidator<CreateEngineeringRequestDto> createValidator,
        IValidator<UpdateEngineeringRequestDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("mine")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> GetMine([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _service.GetMyRequestsAsync(query, cancellationToken));

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.COO}")]
    public async Task<IActionResult> GetAll([FromQuery] EngineeringRequestQuery query, CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> Create([FromBody] CreateEngineeringRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { success = false, message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _service.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEngineeringRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { success = false, message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.SubmitAsync(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteDraftAsync(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
