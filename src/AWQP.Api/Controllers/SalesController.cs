using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using AWQP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,SalesTeam,ProductionManager")]
public sealed class SalesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SalesController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("rfqs")]
    public async Task<IActionResult> Rfqs(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<RequestForQuotation>().Query()
            .Include(x => x.Customer)
            .Select(x => new { x.Id, x.RfqNumber, Customer = x.Customer.Name, x.RequestedDateUtc, x.Status })
            .ToListAsync(cancellationToken));

    [HttpPost("rfqs")]
    public async Task<IActionResult> CreateRfq(CreateRfqRequest request, CancellationToken cancellationToken)
    {
        var rfq = new RequestForQuotation { RfqNumber = request.RfqNumber, CustomerId = request.CustomerId, Status = SalesStatus.Submitted };
        foreach (var line in request.Lines)
        {
            rfq.Lines.Add(new RequestForQuotationLine { ProductId = line.ProductId, Quantity = line.Quantity, RequiredDateUtc = line.RequiredDateUtc, Notes = line.Notes });
        }
        await _unitOfWork.Repository<RequestForQuotation>().AddAsync(rfq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(rfq);
    }

    [HttpPost("quotations")]
    public async Task<IActionResult> CreateQuotation(CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var quotation = new Quotation
        {
            QuotationNumber = request.QuotationNumber,
            RequestForQuotationId = request.RequestForQuotationId,
            Currency = request.Currency,
            ApprovalStatus = SalesStatus.Submitted,
            TotalAmount = request.Lines.Sum(x => x.Quantity * x.UnitPrice)
        };
        foreach (var line in request.Lines)
        {
            quotation.Lines.Add(new QuotationLine { ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice });
        }
        await _unitOfWork.Repository<Quotation>().AddAsync(quotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(quotation);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateSalesOrder(CreateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new SalesOrder
        {
            SalesOrderNumber = request.SalesOrderNumber,
            CustomerId = request.CustomerId,
            QuotationId = request.QuotationId,
            Status = SalesStatus.Approved
        };
        foreach (var line in request.Lines)
        {
            order.Lines.Add(new SalesOrderLine { ProductId = line.ProductId, Quantity = line.Quantity, PromiseDateUtc = line.PromiseDateUtc });
        }
        await _unitOfWork.Repository<SalesOrder>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(order);
    }
}
