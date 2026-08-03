using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/shipping")]
[Authorize(Roles = "Admin,WarehouseStaff,QualityEngineer,SalesTeam")]
public sealed class ShippingController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ShippingController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpPost("packaging-batches")]
    public async Task<IActionResult> CreatePackagingBatch(CreatePackagingBatchRequest request, CancellationToken cancellationToken)
    {
        var batch = new PackagingBatch
        {
            PackagingBatchNumber = request.PackagingBatchNumber,
            WorkOrderId = request.WorkOrderId,
            PackagingOperator = request.PackagingOperator,
            VacuumSealValidated = request.VacuumSealValidated,
            PackagingInspectionResult = request.PackagingInspectionResult
        };
        await _unitOfWork.Repository<PackagingBatch>().AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(batch);
    }

    [HttpGet("shipments")]
    public async Task<IActionResult> Shipments(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<Shipment>().Query()
            .Include(x => x.Customer)
            .Select(x => new { x.Id, x.ShipmentNumber, Customer = x.Customer.Name, x.PlannedShipDateUtc, x.DispatchApprovalStatus, x.ShipmentStatus, x.TrackingNumber })
            .ToListAsync(cancellationToken));

    [HttpPost("shipments")]
    public async Task<IActionResult> CreateShipment(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = new Shipment
        {
            ShipmentNumber = request.ShipmentNumber,
            CustomerId = request.CustomerId,
            PlannedShipDateUtc = request.PlannedShipDateUtc,
            DispatchApprovalStatus = request.DispatchApprovalStatus,
            ShipmentStatus = "Pending"
        };
        foreach (var line in request.Lines)
        {
            shipment.Lines.Add(new ShipmentLine { ProductSerialId = line.ProductSerialId, PackagingBatchId = line.PackagingBatchId });
        }
        await _unitOfWork.Repository<Shipment>().AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(shipment);
    }
}
