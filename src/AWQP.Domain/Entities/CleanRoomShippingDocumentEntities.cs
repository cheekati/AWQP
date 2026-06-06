using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class CleanRoom : AuditableEntity
{
    public string CleanRoomCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string CleanRoomClass { get; set; } = "Class 1000";
    public decimal TemperatureMinC { get; set; } = 20;
    public decimal TemperatureMaxC { get; set; } = 24;
    public decimal HumidityMinPercent { get; set; } = 40;
    public decimal HumidityMaxPercent { get; set; } = 60;
    public int ParticleCountMax { get; set; } = 1000;
}

public sealed class EnvironmentalReading : AuditableEntity
{
    public Guid CleanRoomId { get; set; }
    public CleanRoom CleanRoom { get; set; } = default!;
    public DateTime ReadingTimeUtc { get; set; } = DateTime.UtcNow;
    public decimal TemperatureC { get; set; }
    public decimal HumidityPercent { get; set; }
    public int ParticleCount { get; set; }
    public ComplianceStatus ComplianceStatus { get; set; }
}

public sealed class CleanRoomAccessLog : AuditableEntity
{
    public Guid CleanRoomId { get; set; }
    public CleanRoom CleanRoom { get; set; } = default!;
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public DateTime EntryTimeUtc { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
    public string GowningChecklistStatus { get; set; } = "Passed";
}

public sealed class PackagingBatch : AuditableEntity
{
    public string PackagingBatchNumber { get; set; } = default!;
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = default!;
    public string PackagingOperator { get; set; } = default!;
    public DateTime PackagingDateUtc { get; set; } = DateTime.UtcNow;
    public bool VacuumSealValidated { get; set; }
    public string PackagingInspectionResult { get; set; } = "Pending";
}

public sealed class Shipment : AuditableEntity
{
    public string ShipmentNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public DateTime PlannedShipDateUtc { get; set; }
    public string DispatchApprovalStatus { get; set; } = "Pending";
    public string ShipmentStatus { get; set; } = "Pending";
    public string? TrackingNumber { get; set; }
    public ICollection<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
}

public sealed class ShipmentLine : AuditableEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = default!;
    public Guid ProductSerialId { get; set; }
    public ProductSerial ProductSerial { get; set; } = default!;
    public Guid PackagingBatchId { get; set; }
    public PackagingBatch PackagingBatch { get; set; } = default!;
}

public sealed class ManagedDocument : AuditableEntity
{
    public string DocumentNumber { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Revision { get; set; } = "A";
    public string StorageUri { get; set; } = default!;
    public DateTime EffectiveDateUtc { get; set; } = DateTime.UtcNow;
}
