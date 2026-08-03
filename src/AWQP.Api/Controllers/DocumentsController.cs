using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DocumentsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> Documents(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<ManagedDocument>().Query()
            .Select(x => new { x.Id, x.DocumentNumber, x.DocumentType, x.Title, x.Revision, x.EffectiveDateUtc })
            .ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,QualityEngineer,ProductionManager,ProductionEngineer")]
    public async Task<IActionResult> Create(CreateManagedDocumentRequest request, CancellationToken cancellationToken)
    {
        var document = new ManagedDocument
        {
            DocumentNumber = request.DocumentNumber,
            DocumentType = request.DocumentType,
            Title = request.Title,
            Revision = request.Revision,
            StorageUri = request.StorageUri,
            EffectiveDateUtc = request.EffectiveDateUtc
        };
        await _unitOfWork.Repository<ManagedDocument>().AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(document);
    }
}
