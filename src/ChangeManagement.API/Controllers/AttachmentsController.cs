using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.API.Controllers;

[ApiController]
[Route("api/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("{erId:guid}")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    [RequestSizeLimit(524_288_000)] // 500 MB for bulk image uploads
    public async Task<IActionResult> Upload(Guid erId, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.UploadAsync(erId, Request.Form.Files, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{erId:guid}/{attachmentId:guid}")]
    [Authorize(Roles = $"{AppRoles.Requester},{AppRoles.Administrator}")]
    public async Task<IActionResult> Delete(Guid erId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.DeleteAsync(erId, attachmentId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{erId:guid}/{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid erId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var file = await _attachmentService.DownloadAsync(erId, attachmentId, cancellationToken);
        if (file is null) return NotFound();
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }
}
