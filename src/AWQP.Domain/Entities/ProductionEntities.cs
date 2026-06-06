using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class Machine : AuditableEntity
{
    public string MachineCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string WorkCenter { get; set; } = default!;
    public bool IsCleanRoomQualified { get; set; }
}

public sealed class Shift : AuditableEntity
{
    public string Name { get; set; } = default!;
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
}

public sealed class ProductionSchedule : AuditableEntity
{
    public string ScheduleNumber { get; set; } = default!;
    public DateOnly ScheduleDate { get; set; }
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = default!;
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public sealed class WorkOrder : AuditableEntity
{
    public string WorkOrderNumber { get; set; } = default!;
    public string BatchNumber { get; set; } = default!;
    public string LotNumber { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid? SalesOrderLineId { get; set; }
    public SalesOrderLine? SalesOrderLine { get; set; }
    public Guid? ProductionScheduleId { get; set; }
    public ProductionSchedule? ProductionSchedule { get; set; }
    public int QuantityPlanned { get; set; }
    public int QuantityProduced { get; set; }
    public int QuantityRejected { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;
    public ICollection<ManufacturingOperation> Operations { get; set; } = new List<ManufacturingOperation>();
    public ICollection<ProductSerial> ProductSerials { get; set; } = new List<ProductSerial>();
}

public sealed class ManufacturingOperation : AuditableEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = default!;
    public ProcessStageType Stage { get; set; }
    public int Sequence { get; set; }
    public string OperatorName { get; set; } = default!;
    public Guid? MachineId { get; set; }
    public Machine? Machine { get; set; }
    public DateTime? StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public int QuantityPlanned { get; set; }
    public int QuantityProduced { get; set; }
    public int QuantityRejected { get; set; }
    public decimal YieldPercentage => QuantityProduced + QuantityRejected == 0 ? 0 : Math.Round((decimal)QuantityProduced / (QuantityProduced + QuantityRejected) * 100m, 2);
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;
    public string? Remarks { get; set; }
}

public sealed class ProductSerial : AuditableEntity
{
    public string SerialNumber { get; set; } = default!;
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = default!;
    public DateTime ManufacturingDateUtc { get; set; } = DateTime.UtcNow;
    public string CurrentStatus { get; set; } = "Manufacturing";
}
