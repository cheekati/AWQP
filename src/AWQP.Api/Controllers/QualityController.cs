using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/qms")]
[Authorize(Policy = "Quality")]
public sealed class QualityController : ControllerBase
{
    private readonly IQualityService _qualityService;
    public QualityController(IQualityService qualityService) => _qualityService = qualityService;

    [HttpPost("inspections")]
    public async Task<IActionResult> CreateInspection(CreateInspectionRequest request, CancellationToken cancellationToken) => Ok(await _qualityService.CreateInspectionAsync(request, cancellationToken));
}
