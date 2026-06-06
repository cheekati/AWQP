using AWQP.Application.Common;
using AWQP.Domain.CleanRoom;
using AWQP.Domain.Common;
using AWQP.Domain.Inventory;
using AWQP.Domain.Logistics;
using AWQP.Domain.Production;
using AWQP.Domain.Quality;
using AWQP.Domain.Traceability;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Application.Services;

public sealed class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<ProductionDashboardDto> GetProductionDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var operations = unitOfWork.Repository<ManufacturingOperation>().Query();
        var dailyProduction = await unitOfWork.Repository<WorkOrder>().Query()
            .Where(x => x.ManufacturingDate == today)
            .SumAsync(x => (decimal?)x.QuantityProduced, cancellationToken) ?? 0;
        var completed = await operations.CountAsync(x => x.Status == OperationStatus.Completed, cancellationToken);
        var total = await operations.CountAsync(cancellationToken);
        var averageYield = await unitOfWork.Repository<WorkOrder>().Query()
            .AverageAsync(x => (decimal?)x.YieldPercentage, cancellationToken) ?? 0;
        var utilization = total == 0 ? 0 : Math.Round(completed / (decimal)total * 100, 2);
        return new ProductionDashboardDto(dailyProduction, utilization, utilization, averageYield);
    }

    public async Task<QualityDashboardDto> GetQualityDashboardAsync(CancellationToken cancellationToken = default)
    {
        var ncrCount = await unitOfWork.Repository<NonConformanceReport>().Query()
            .CountAsync(x => x.Status != SalesDocumentStatus.Approved, cancellationToken);
        var capaCount = await unitOfWork.Repository<CorrectivePreventiveAction>().Query()
            .CountAsync(x => x.Status != CapaStatus.Closed, cancellationToken);
        var defects = await unitOfWork.Repository<Defect>().Query()
            .GroupBy(x => x.DefectCode)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        return new QualityDashboardDto(ncrCount, capaCount, defects);
    }

    public async Task<InventoryDashboardDto> GetInventoryDashboardAsync(CancellationToken cancellationToken = default)
    {
        var query = unitOfWork.Repository<InventoryItem>().Query();
        var stock = await query.SumAsync(x => (decimal?)x.QuantityOnHand, cancellationToken) ?? 0;
        var value = await query.SumAsync(x => (decimal?)(x.QuantityOnHand * x.StandardCost), cancellationToken) ?? 0;
        var reorder = await query.CountAsync(x => x.QuantityOnHand <= x.ReorderLevel, cancellationToken);
        return new InventoryDashboardDto(stock, value, reorder);
    }

    public async Task<CleanRoomDashboardDto> GetCleanRoomDashboardAsync(CancellationToken cancellationToken = default)
    {
        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var readings = unitOfWork.Repository<CleanRoomEnvironmentalReading>().Query().Where(x => x.RecordedAt >= since);
        var averageTemp = await readings.AverageAsync(x => (decimal?)x.TemperatureC, cancellationToken) ?? 0;
        var averageHumidity = await readings.AverageAsync(x => (decimal?)x.HumidityPercent, cancellationToken) ?? 0;
        var maxParticles = await readings.MaxAsync(x => (int?)x.ParticleCountPerCubicFoot, cancellationToken) ?? 0;
        var compliant = !await readings.AnyAsync(x => !x.IsCompliant, cancellationToken);
        return new CleanRoomDashboardDto(averageTemp, averageHumidity, maxParticles, compliant);
    }

    public async Task<ShippingDashboardDto> GetShippingDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var pending = await unitOfWork.Repository<Shipment>().Query().CountAsync(x => x.Status == ShipmentStatus.PendingInspection, cancellationToken);
        var delivered = await unitOfWork.Repository<Shipment>().Query().CountAsync(x => x.DeliveredAt != null && x.DeliveredAt.Value.Date == today, cancellationToken);
        var statuses = await unitOfWork.Repository<Shipment>().Query()
            .GroupBy(x => x.Status)
            .Select(x => new { Key = x.Key.ToString(), Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        return new ShippingDashboardDto(pending, delivered, statuses);
    }
}

public sealed class TraceabilityService(IUnitOfWork unitOfWork) : ITraceabilityService
{
    public async Task<GenealogyDto?> GetGenealogyAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var serial = await unitOfWork.Repository<ProductSerial>()
            .Query()
            .Include(x => x.WorkOrder)
            .Include(x => x.ChildLinks).ThenInclude(x => x.ChildProductSerial)
            .SingleOrDefaultAsync(x => x.SerialNumber == serialNumber, cancellationToken);

        if (serial is null)
        {
            return null;
        }

        var events = await unitOfWork.Repository<TraceabilityEvent>()
            .Query()
            .Include(x => x.ManufacturingOperation)
            .ThenInclude(x => x!.WorkOrder)
            .Where(x => x.ProductSerialId == serial.Id)
            .OrderBy(x => x.EventAt)
            .Select(x => new GenealogyEventDto(
                x.EventType,
                x.EventReference,
                x.EventAt,
                x.ManufacturingOperation!.WorkOrder!.WorkOrderNumber,
                x.ManufacturingOperation.Stage.ToString()))
            .ToListAsync(cancellationToken);

        var children = serial.ChildLinks
            .Where(x => x.ChildProductSerial is not null)
            .Select(x => new GenealogyNodeDto(
                x.ChildProductSerial!.SerialNumber,
                x.ChildProductSerial.BatchNumber,
                x.ChildProductSerial.LotNumber,
                x.ChildProductSerial.CurrentStatus,
                []))
            .ToList();

        return new GenealogyDto(serial.SerialNumber, serial.BatchNumber, serial.LotNumber, serial.ManufacturingDate, events, children);
    }
}
