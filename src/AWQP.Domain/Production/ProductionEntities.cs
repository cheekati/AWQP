using AWQP.Domain.Common;
using AWQP.Domain.Products;
using AWQP.Domain.Sales;

namespace AWQP.Domain.Production;

public sealed class Machine : NamedEntity
{
    public string WorkCenter { get; set; } = string.Empty;
    public decimal RatedCapacityPerHour { get; set; }
    public bool IsCleanRoomQualified { get; set; }
    public DateTimeOffset? LastCalibrationAt { get; set; }
}

public sealed class Shift : NamedEntity
{
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
}

public sealed class WorkOrder : BaseEntity
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public Guid SalesOrderLineId { get; set; }
    public SalesOrderLine? SalesOrderLine { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public decimal YieldPercentage { get; set; }
    public DateOnly ManufacturingDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;
    public ICollection<ManufacturingOperation> Operations { get; set; } = [];
}

public sealed class ProductionSchedule : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public DateTimeOffset PlannedStart { get; set; }
    public DateTimeOffset PlannedEnd { get; set; }
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public string Priority { get; set; } = "Normal";
}

public sealed class ResourceAllocation : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Guid MachineId { get; set; }
    public Machine? Machine { get; set; }
    public string OperatorUserId { get; set; } = string.Empty;
    public DateTimeOffset AllocatedFrom { get; set; }
    public DateTimeOffset AllocatedTo { get; set; }
}

public sealed class ManufacturingOperation : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public OperationStage Stage { get; set; }
    public int Sequence { get; set; }
    public string OperatorUserId { get; set; } = string.Empty;
    public Guid? MachineId { get; set; }
    public Machine? Machine { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public decimal YieldPercentage { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Queued;
    public string? Remarks { get; set; }
}
