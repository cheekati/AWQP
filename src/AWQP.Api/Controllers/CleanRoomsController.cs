using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/clean-rooms")]
[Authorize(Policy = "CleanRoom")]
public sealed class CleanRoomsController : ControllerBase
{
    private readonly ICleanRoomService _cleanRoomService;
    public CleanRoomsController(ICleanRoomService cleanRoomService) => _cleanRoomService = cleanRoomService;

    [HttpPost("readings")]
    public async Task<IActionResult> RecordReading(CreateEnvironmentalReadingRequest request, CancellationToken cancellationToken) => Ok(await _cleanRoomService.RecordReadingAsync(request, cancellationToken));
}
