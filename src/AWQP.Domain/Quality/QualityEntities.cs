using AWQP.Domain.Common;
using AWQP.Domain.Production;

namespace AWQP.Domain.Quality;

public sealed class InspectionRecord : BaseEntity
{
    public string InspectionNumber { get; set; } = string.Empty;
    public InspectionType InspectionType { get; set; }
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Guid? ManufacturingOperationId { get; set; }
    public ManufacturingOperation? ManufacturingOperation { get; set; }
    public string InspectorUserId { get; set; } = string.Empty;
    public DateTimeOffset InspectedAt { get; set; } = DateTimeOffset.UtcNow;
    public InspectionDisposition Disposition { get; set; } = InspectionDisposition.Pending;
    public string? Remarks { get; set; }
    public ICollection<InspectionParameterResult> ParameterResults { get; set; } = [];
}

public sealed class InspectionParameterResult : BaseEntity
{
    public Guid InspectionRecordId { get; set; }
    public InspectionRecord? InspectionRecord { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public string ActualValue { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool IsPass { get; set; }
}

public sealed class Defect : BaseEntity
{
    public Guid InspectionRecordId { get; set; }
    public InspectionRecord? InspectionRecord { get; set; }
    public string DefectCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public decimal QuantityAffected { get; set; }
}

public sealed class NonConformanceReport : BaseEntity
{
    public string NcrNumber { get; set; } = string.Empty;
    public Guid InspectionRecordId { get; set; }
    public InspectionRecord? InspectionRecord { get; set; }
    public string ProblemStatement { get; set; } = string.Empty;
    public string ContainmentAction { get; set; } = string.Empty;
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Submitted;
    public ICollection<CorrectivePreventiveAction> Capas { get; set; } = [];
}

public sealed class CorrectivePreventiveAction : BaseEntity
{
    public string CapaNumber { get; set; } = string.Empty;
    public Guid NonConformanceReportId { get; set; }
    public NonConformanceReport? NonConformanceReport { get; set; }
    public string ActionPlan { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public CapaStatus Status { get; set; } = CapaStatus.Open;
}

public sealed class RootCauseAnalysis : BaseEntity
{
    public Guid CorrectivePreventiveActionId { get; set; }
    public CorrectivePreventiveAction? CorrectivePreventiveAction { get; set; }
    public string Method { get; set; } = "5-Why";
    public string RootCause { get; set; } = string.Empty;
    public string VerificationEvidence { get; set; } = string.Empty;
}
