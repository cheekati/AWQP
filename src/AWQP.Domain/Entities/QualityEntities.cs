using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class InspectionRecord : AuditableEntity
{
    public string InspectionNumber { get; set; } = default!;
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Guid? ProductSerialId { get; set; }
    public ProductSerial? ProductSerial { get; set; }
    public ProcessStageType InspectionStage { get; set; }
    public string InspectorName { get; set; } = default!;
    public DateTime InspectionDateUtc { get; set; } = DateTime.UtcNow;
    public QualityDisposition Disposition { get; set; } = QualityDisposition.Accepted;
    public ICollection<InspectionMeasurement> Measurements { get; set; } = new List<InspectionMeasurement>();
}

public sealed class InspectionMeasurement : AuditableEntity
{
    public Guid InspectionRecordId { get; set; }
    public InspectionRecord InspectionRecord { get; set; } = default!;
    public string ParameterName { get; set; } = default!;
    public string Specification { get; set; } = default!;
    public string MeasuredValue { get; set; } = default!;
    public bool IsPass { get; set; }
}

public sealed class DefectRecord : AuditableEntity
{
    public Guid InspectionRecordId { get; set; }
    public InspectionRecord InspectionRecord { get; set; } = default!;
    public string DefectCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Severity { get; set; } = "Major";
}

public sealed class NonConformanceReport : AuditableEntity
{
    public string NcrNumber { get; set; } = default!;
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public string ProblemStatement { get; set; } = default!;
    public QualityDisposition Disposition { get; set; } = QualityDisposition.Rework;
    public string Status { get; set; } = "Open";
}

public sealed class CapaRecord : AuditableEntity
{
    public string CapaNumber { get; set; } = default!;
    public Guid NonConformanceReportId { get; set; }
    public NonConformanceReport NonConformanceReport { get; set; } = default!;
    public string RootCause { get; set; } = default!;
    public string CorrectiveAction { get; set; } = default!;
    public string PreventiveAction { get; set; } = default!;
    public string Status { get; set; } = "Open";
}
