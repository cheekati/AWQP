using System.Security.Claims;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Models;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/engineering-requests")]
[Authorize]
public class EngineeringRequestsController : ControllerBase
{
    private readonly IErService _ers;
    private readonly IAuthService _auth;

    public EngineeringRequestsController(IErService ers, IAuthService auth)
    {
        _ers = ers;
        _auth = auth;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ErListItemDto>>> List()
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        return Ok(await _ers.ListForUserAsync(user));
    }

    [HttpGet("verification-queue")]
    public async Task<ActionResult<IEnumerable<ErListItemDto>>> VerificationQueue()
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        return Ok(await _ers.ListVerificationQueueAsync(user));
    }

    [HttpGet("coo-queue")]
    public async Task<ActionResult<IEnumerable<ErListItemDto>>> CooQueue()
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        return Ok(await _ers.ListCooQueueAsync(user));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ErDetailDto>> Get(int id)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        var er = await _ers.GetByIdAsync(id, user);
        return er is null ? NotFound() : Ok(er);
    }

    [HttpPost]
    public async Task<ActionResult<ErDetailDto>> Create([FromBody] CreateErRequest request)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var er = await _ers.CreateAsync(request, user);
            return CreatedAtAction(nameof(Get), new { id = er.Id }, er);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ErDetailDto>> Update(int id, [FromBody] UpdateErRequest request)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var er = await _ers.UpdateAsync(id, request, user);
            return er is null ? NotFound() : Ok(er);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/verify")]
    public async Task<ActionResult<ErDetailDto>> Verify(int id, [FromBody] VerificationActionRequest request)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var er = await _ers.VerifyAsync(id, request, user);
            return er is null ? NotFound() : Ok(er);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/coo")]
    public async Task<ActionResult<ErDetailDto>> Coo(int id, [FromBody] CooActionRequest request)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var er = await _ers.CooActAsync(id, request, user);
            return er is null ? NotFound() : Ok(er);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/attachments")]
    [RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<AttachmentDto>> Upload(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required." });

        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var attachment = await _ers.AddAttachmentAsync(id, file, user);
            return attachment is null ? NotFound() : Ok(attachment);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
    {
        var user = await CurrentUser();
        if (user is null) return Unauthorized();
        try
        {
            var ok = await _ers.DeleteAttachmentAsync(id, attachmentId, user);
            return ok ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<User?> CurrentUser()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id)) return null;
        return await _auth.GetUserAsync(id);
    }
}
