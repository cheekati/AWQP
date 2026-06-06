using AWQP.Application.Common;
using AWQP.Application.Services;
using AWQP.Domain.CleanRoom;
using AWQP.Domain.Inventory;
using AWQP.Domain.Logistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize(Policy = "Production")]
public sealed class WorkOrdersController(WorkOrderService workOrderService) : ControllerBase
{
    [HttpGet("open")]
    public Task<IReadOnlyList<WorkOrderDto>> GetOpen(CancellationToken cancellationToken)
    {
        return workOrderService.GetOpenWorkOrdersAsync(cancellationToken);
    }

    [HttpPost]
    public Task<WorkOrderDto> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        return workOrderService.CreateWorkOrderAsync(request, cancellationToken);
    }

    [HttpPost("operations/complete")]
    public Task<ManufacturingOperationDto> CompleteOperation(CompleteOperationRequest request, CancellationToken cancellationToken)
    {
        return workOrderService.CompleteOperationAsync(request, cancellationToken);
    }
}

[ApiController]
[Route("api/clean-room")]
[Authorize(Policy = "CleanRoom")]
public sealed class CleanRoomController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("readings")]
    public async Task<CleanRoomEnvironmentalReading> AddReading(CleanRoomEnvironmentalReading reading, CancellationToken cancellationToken)
    {
        reading.IsCompliant = reading.ParticleCountPerCubicFoot <= 1000;
        await unitOfWork.Repository<CleanRoomEnvironmentalReading>().AddAsync(reading, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reading;
    }

    [HttpPost("access-logs")]
    public async Task<CleanRoomAccessLog> AddAccessLog(CleanRoomAccessLog accessLog, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<CleanRoomAccessLog>().AddAsync(accessLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return accessLog;
    }
}

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "Warehouse")]
public sealed class InventoryController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("items")]
    public async Task<IReadOnlyList<InventoryItem>> GetItems(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<InventoryItem>().ListAsync(cancellationToken: cancellationToken);
    }

    [HttpPost("transactions")]
    public async Task<InventoryTransaction> CreateTransaction(InventoryTransaction transaction, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<InventoryTransaction>().AddAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return transaction;
    }
}

[ApiController]
[Route("api/shipping")]
[Authorize(Roles = "Admin,Warehouse Staff,Quality Engineer,Sales Team")]
public sealed class ShippingController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("shipments")]
    public async Task<IReadOnlyList<Shipment>> GetShipments(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<Shipment>().ListAsync(cancellationToken: cancellationToken);
    }

    [HttpPost("shipments")]
    public async Task<Shipment> CreateShipment(Shipment shipment, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<Shipment>().AddAsync(shipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shipment;
    }
}
