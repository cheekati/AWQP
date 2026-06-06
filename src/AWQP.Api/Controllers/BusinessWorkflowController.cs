using AWQP.Application.Common;
using AWQP.Domain.Documents;
using AWQP.Domain.Logistics;
using AWQP.Domain.Quality;
using AWQP.Domain.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Sales Team,Production Manager")]
public sealed class SalesController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("rfqs")]
    public async Task<RequestForQuote> CreateRfq(RequestForQuote rfq, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<RequestForQuote>().AddAsync(rfq, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return rfq;
    }

    [HttpPost("quotations")]
    public async Task<Quotation> CreateQuotation(Quotation quotation, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<Quotation>().AddAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return quotation;
    }

    [HttpPost("orders")]
    public async Task<SalesOrder> CreateSalesOrder(SalesOrder salesOrder, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<SalesOrder>().AddAsync(salesOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return salesOrder;
    }
}

[ApiController]
[Route("api/quality")]
[Authorize(Policy = "Quality")]
public sealed class QualityController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("inspections")]
    public async Task<InspectionRecord> CreateInspection(InspectionRecord inspection, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<InspectionRecord>().AddAsync(inspection, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return inspection;
    }

    [HttpPost("ncrs")]
    public async Task<NonConformanceReport> CreateNcr(NonConformanceReport ncr, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<NonConformanceReport>().AddAsync(ncr, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ncr;
    }

    [HttpPost("capas")]
    public async Task<CorrectivePreventiveAction> CreateCapa(CorrectivePreventiveAction capa, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<CorrectivePreventiveAction>().AddAsync(capa, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return capa;
    }
}

[ApiController]
[Route("api/packaging")]
[Authorize(Roles = "Admin,Clean Room Operator,Quality Engineer")]
public sealed class PackagingController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("vacuum-records")]
    public async Task<VacuumPackagingRecord> CreateVacuumRecord(VacuumPackagingRecord record, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<VacuumPackagingRecord>().AddAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return record;
    }
}

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ManagedDocument>> Get(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<ManagedDocument>().ListAsync(cancellationToken: cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Production Engineer,Quality Engineer")]
    public async Task<ManagedDocument> Create(ManagedDocument document, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<ManagedDocument>().AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return document;
    }
}
