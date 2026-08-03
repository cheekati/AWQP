using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

/// <summary>
/// EHS Work Permit (Hot/Cold work). Distinct from mes.WorkOrder manufacturing orders.
/// </summary>
public sealed class WorkPermit : AuditableEntity
{
    public long PermitNumber { get; set; }
    public string WorkType { get; set; } = "COLD WORK";
    public string Status { get; set; } = "APPROVED";
    public string Department { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string PlantLocation { get; set; } = string.Empty;
    public string WorkInfo { get; set; } = string.Empty;
    public string? DetailDescription { get; set; }
    public DateTime? WorkScheduleDateFrom { get; set; }
    public DateTime? WorkScheduleDateTo { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierPic { get; set; }
    public string? SupContactNo { get; set; }
    public string? PoNumber { get; set; }
    public decimal? PoCost { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool NoPoPr { get; set; }
    public bool Po { get; set; }
    public bool Pr { get; set; }
    public string? InProgressRemarks { get; set; }
    public string? OnHoldRemarks { get; set; }
    public string? CompletedRemarks { get; set; }
    public string? InProgressBy { get; set; }
    public DateTime? InProgressOn { get; set; }
    public string? OnHoldBy { get; set; }
    public DateTime? OnHoldOn { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CompletedOn { get; set; }
    public string? SupportingDocument6 { get; set; }
    public string? UploadDocument1 { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DailyAtmosphericReading> DailyAtmosphericReadings { get; set; } = new List<DailyAtmosphericReading>();
}

/// <summary>
/// One atmospheric test reading per calendar day for Hot Work / Confined Space permits.
/// </summary>
public sealed class DailyAtmosphericReading : AuditableEntity
{
    public Guid WorkPermitId { get; set; }
    public WorkPermit WorkPermit { get; set; } = default!;
    public DateOnly ReadingDate { get; set; }
    public DateTime ReadingDateTimeUtc { get; set; } = DateTime.UtcNow;
    public decimal OxygenContentPercent { get; set; }
    public decimal ToxicGasH2SPpm { get; set; }
    public decimal CarbonMonoxidePpm { get; set; }
    public decimal CombustibleGasLelPercent { get; set; }
    public string? PicName { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Master acceptable ranges for atmospheric testing parameters (shown on front page).
/// </summary>
public sealed class AtmosphericParameterRange : AuditableEntity
{
    public string ParameterCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal? MinAcceptable { get; set; }
    public decimal? MaxAcceptable { get; set; }
    public string DisplayRange { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
