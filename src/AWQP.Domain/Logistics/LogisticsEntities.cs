using AWQP.Domain.Common;
using AWQP.Domain.Customers;
using AWQP.Domain.Production;
using AWQP.Domain.Quality;

namespace AWQP.Domain.Logistics;

public sealed class VacuumPackagingRecord : BaseEntity
{
    public string PackagingBatchNumber { get; set; } = string.Empty;
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public string PackagingOperatorUserId { get; set; } = string.Empty;
    public DateTimeOffset PackagingDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal VacuumPressureKpa { get; set; }
    public bool VacuumSealValidated { get; set; }
    public bool PackagingInspectionPassed { get; set; }
    public string? Remarks { get; set; }
}

public sealed class ShippingInspection : BaseEntity
{
    public string ShippingInspectionNumber { get; set; } = string.Empty;
    public Guid VacuumPackagingRecordId { get; set; }
    public VacuumPackagingRecord? VacuumPackagingRecord { get; set; }
    public Guid? InspectionRecordId { get; set; }
    public InspectionRecord? InspectionRecord { get; set; }
    public bool DispatchApproved { get; set; }
    public string ApprovedByUserId { get; set; } = string.Empty;
    public DateTimeOffset? ApprovedAt { get; set; }
}

public sealed class Shipment : BaseEntity
{
    public string ShipmentNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.PendingInspection;
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string ExportDocumentNumber { get; set; } = string.Empty;
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public ICollection<ShipmentLine> Lines { get; set; } = [];
}

public sealed class ShipmentLine : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }
    public Guid VacuumPackagingRecordId { get; set; }
    public VacuumPackagingRecord? VacuumPackagingRecord { get; set; }
    public string ProductSerialNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
