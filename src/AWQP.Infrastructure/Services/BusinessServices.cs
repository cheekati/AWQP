using AutoMapper;
using AutoMapper.QueryableExtensions;
using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using AWQP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer { CustomerCode = request.CustomerCode, Name = request.Name, TaxNumber = request.TaxNumber, Website = request.Website };
        foreach (var industry in request.Industries.Distinct())
        {
            customer.Industries.Add(new CustomerIndustry { IndustrySegment = industry });
        }
        await _unitOfWork.Repository<Customer>().AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IReadOnlyCollection<CustomerDto>> ListAsync(CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<Customer>().Query().Include(x => x.Industries).ProjectTo<CustomerDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}

public sealed class ManufacturingService : IManufacturingService
{
    private static readonly ProcessStageType[] StandardStages =
    {
        ProcessStageType.RawMaterialPreparation,
        ProcessStageType.QuartzProcessing,
        ProcessStageType.Forming,
        ProcessStageType.FiringProcess,
        ProcessStageType.Inspection,
        ProcessStageType.CleanRoomEntry,
        ProcessStageType.FinalCleaning,
        ProcessStageType.VacuumPackaging,
        ProcessStageType.ShippingInspection,
        ProcessStageType.Dispatch
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ManufacturingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        var workOrder = new WorkOrder
        {
            WorkOrderNumber = request.WorkOrderNumber,
            BatchNumber = request.BatchNumber,
            LotNumber = request.LotNumber,
            ProductId = request.ProductId,
            QuantityPlanned = request.QuantityPlanned,
            Status = WorkOrderStatus.Released
        };
        for (var i = 0; i < StandardStages.Length; i++)
        {
            workOrder.Operations.Add(new ManufacturingOperation
            {
                Stage = StandardStages[i],
                Sequence = i + 1,
                QuantityPlanned = request.QuantityPlanned,
                Status = WorkOrderStatus.Planned,
                OperatorName = "Unassigned"
            });
        }
        for (var i = 1; i <= request.QuantityPlanned; i++)
        {
            workOrder.ProductSerials.Add(new ProductSerial { SerialNumber = $"{request.WorkOrderNumber}-{i:0000}" });
        }
        await _unitOfWork.Repository<WorkOrder>().AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await _unitOfWork.Repository<WorkOrder>().Query().Include(x => x.Product).Where(x => x.Id == workOrder.Id).ProjectTo<WorkOrderDto>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }

    public async Task StartOperationAsync(OperationStartRequest request, CancellationToken cancellationToken = default)
    {
        var operation = await _unitOfWork.Repository<ManufacturingOperation>().GetByIdAsync(request.OperationId, cancellationToken) ?? throw new InvalidOperationException("Operation not found.");
        operation.OperatorName = request.OperatorName;
        operation.MachineId = request.MachineId;
        operation.StartTimeUtc = DateTime.UtcNow;
        operation.Status = WorkOrderStatus.InProgress;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteOperationAsync(OperationCompleteRequest request, CancellationToken cancellationToken = default)
    {
        var operation = await _unitOfWork.Repository<ManufacturingOperation>().GetByIdAsync(request.OperationId, cancellationToken) ?? throw new InvalidOperationException("Operation not found.");
        operation.QuantityProduced = request.QuantityProduced;
        operation.QuantityRejected = request.QuantityRejected;
        operation.EndTimeUtc = DateTime.UtcNow;
        operation.Remarks = request.Remarks;
        operation.Status = WorkOrderStatus.Completed;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TraceabilityDto?> GetGenealogyAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var serial = await _unitOfWork.Repository<ProductSerial>().Query()
            .Include(x => x.WorkOrder).ThenInclude(x => x.Product)
            .Include(x => x.WorkOrder).ThenInclude(x => x.Operations)
            .FirstOrDefaultAsync(x => x.SerialNumber == serialNumber, cancellationToken);
        if (serial is null) return null;

        var inspections = await _unitOfWork.Repository<InspectionRecord>().Query()
            .Where(x => x.ProductSerialId == serial.Id || x.WorkOrderId == serial.WorkOrderId)
            .Select(x => $"{x.InspectionNumber}:{x.Disposition}")
            .ToListAsync(cancellationToken);
        var packaging = await _unitOfWork.Repository<PackagingBatch>().Query()
            .Where(x => x.WorkOrderId == serial.WorkOrderId)
            .Select(x => x.PackagingBatchNumber)
            .ToListAsync(cancellationToken);
        var shipments = await _unitOfWork.Repository<ShipmentLine>().Query()
            .Include(x => x.Shipment)
            .Where(x => x.ProductSerialId == serial.Id)
            .Select(x => x.Shipment.ShipmentNumber)
            .ToListAsync(cancellationToken);

        return new TraceabilityDto(serial.SerialNumber, serial.WorkOrder.WorkOrderNumber, serial.WorkOrder.BatchNumber, serial.WorkOrder.LotNumber, serial.WorkOrder.Product.ProductCode, serial.ManufacturingDateUtc, serial.WorkOrder.Operations.OrderBy(x => x.Sequence).Select(x => $"{x.Stage}:{x.Status}").ToArray(), inspections, packaging, shipments);
    }
}

public sealed class QualityService : IQualityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QualityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InspectionRecordDto> CreateInspectionAsync(CreateInspectionRequest request, CancellationToken cancellationToken = default)
    {
        var record = new InspectionRecord { InspectionNumber = request.InspectionNumber, WorkOrderId = request.WorkOrderId, ProductSerialId = request.ProductSerialId, InspectionStage = request.InspectionStage, InspectorName = request.InspectorName };
        foreach (var measurement in request.Measurements)
        {
            record.Measurements.Add(new InspectionMeasurement { ParameterName = measurement.ParameterName, Specification = measurement.Specification, MeasuredValue = measurement.MeasuredValue, IsPass = measurement.IsPass });
        }
        record.Disposition = record.Measurements.All(x => x.IsPass) ? QualityDisposition.Accepted : QualityDisposition.Rejected;
        await _unitOfWork.Repository<InspectionRecord>().AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<InspectionRecordDto>(record);
    }
}

public sealed class CleanRoomService : ICleanRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CleanRoomService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EnvironmentalReadingDto> RecordReadingAsync(CreateEnvironmentalReadingRequest request, CancellationToken cancellationToken = default)
    {
        var cleanRoom = await _unitOfWork.Repository<CleanRoom>().GetByIdAsync(request.CleanRoomId, cancellationToken) ?? throw new InvalidOperationException("Clean room not found.");
        var compliant = request.TemperatureC >= cleanRoom.TemperatureMinC && request.TemperatureC <= cleanRoom.TemperatureMaxC && request.HumidityPercent >= cleanRoom.HumidityMinPercent && request.HumidityPercent <= cleanRoom.HumidityMaxPercent && request.ParticleCount <= cleanRoom.ParticleCountMax;
        var reading = new EnvironmentalReading { CleanRoomId = cleanRoom.Id, CleanRoom = cleanRoom, TemperatureC = request.TemperatureC, HumidityPercent = request.HumidityPercent, ParticleCount = request.ParticleCount, ComplianceStatus = compliant ? ComplianceStatus.Compliant : ComplianceStatus.NonCompliant };
        await _unitOfWork.Repository<EnvironmentalReading>().AddAsync(reading, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EnvironmentalReadingDto>(reading);
    }
}

public sealed class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<DashboardDto> GetExecutiveDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var completedOps = await _unitOfWork.Repository<ManufacturingOperation>().Query().CountAsync(x => x.Status == WorkOrderStatus.Completed && x.EndTimeUtc >= today, cancellationToken);
        var rejected = await _unitOfWork.Repository<ManufacturingOperation>().Query().SumAsync(x => (int?)x.QuantityRejected, cancellationToken) ?? 0;
        var produced = await _unitOfWork.Repository<ManufacturingOperation>().Query().SumAsync(x => (int?)x.QuantityProduced, cancellationToken) ?? 0;
        var yield = produced + rejected == 0 ? 0 : Math.Round((decimal)produced / (produced + rejected) * 100, 2);
        var ncrCount = await _unitOfWork.Repository<NonConformanceReport>().Query().CountAsync(x => x.Status != "Closed", cancellationToken);
        var stock = await _unitOfWork.Repository<InventoryStock>().Query().SumAsync(x => (decimal?)x.QuantityOnHand, cancellationToken) ?? 0;
        var nonCompliant = await _unitOfWork.Repository<EnvironmentalReading>().Query().CountAsync(x => x.ComplianceStatus != ComplianceStatus.Compliant && x.ReadingTimeUtc >= today, cancellationToken);
        var pendingShipments = await _unitOfWork.Repository<Shipment>().Query().CountAsync(x => x.ShipmentStatus != "Delivered", cancellationToken);

        return new DashboardDto(
            new[] { new DashboardMetricDto("Daily Completed Operations", completedOps, "ops", "OK"), new DashboardMetricDto("Yield", yield, "%", yield >= 98 ? "OK" : "Watch") },
            new[] { new DashboardMetricDto("Open NCR", ncrCount, "count", ncrCount == 0 ? "OK" : "Action") },
            new[] { new DashboardMetricDto("Current Stock", stock, "units", "OK") },
            new[] { new DashboardMetricDto("Clean Room Exceptions", nonCompliant, "count", nonCompliant == 0 ? "Compliant" : "NonCompliant") },
            new[] { new DashboardMetricDto("Pending Dispatches", pendingShipments, "count", pendingShipments == 0 ? "OK" : "Pending") });
    }
}
