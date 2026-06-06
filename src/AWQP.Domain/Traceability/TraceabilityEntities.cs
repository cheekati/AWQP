using AWQP.Domain.Common;
using AWQP.Domain.Logistics;
using AWQP.Domain.Production;
using AWQP.Domain.Quality;

namespace AWQP.Domain.Traceability;

public sealed class ProductSerial : BaseEntity
{
    public string SerialNumber { get; set; } = string.Empty;
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public DateOnly ManufacturingDate { get; set; }
    public string CurrentStatus { get; set; } = "Manufacturing";
    public ICollection<ProductGenealogyLink> ParentLinks { get; set; } = [];
    public ICollection<ProductGenealogyLink> ChildLinks { get; set; } = [];
}

public sealed class ProductGenealogyLink : BaseEntity
{
    public Guid ParentProductSerialId { get; set; }
    public ProductSerial? ParentProductSerial { get; set; }
    public Guid ChildProductSerialId { get; set; }
    public ProductSerial? ChildProductSerial { get; set; }
    public string RelationshipType { get; set; } = "ConsumedInto";
}

public sealed class TraceabilityEvent : BaseEntity
{
    public Guid ProductSerialId { get; set; }
    public ProductSerial? ProductSerial { get; set; }
    public Guid? ManufacturingOperationId { get; set; }
    public ManufacturingOperation? ManufacturingOperation { get; set; }
    public Guid? InspectionRecordId { get; set; }
    public InspectionRecord? InspectionRecord { get; set; }
    public Guid? VacuumPackagingRecordId { get; set; }
    public VacuumPackagingRecord? VacuumPackagingRecord { get; set; }
    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventReference { get; set; } = string.Empty;
    public DateTimeOffset EventAt { get; set; } = DateTimeOffset.UtcNow;
}
