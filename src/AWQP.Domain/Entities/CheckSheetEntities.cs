using AWQP.Domain.Common;

namespace AWQP.Domain.Entities;

/// <summary>Check Point Master — central definition of check points and their min/max specs.</summary>
public sealed class CheckPointMaster : AuditableEntity
{
    public string CheckPointName { get; set; } = default!;
    public string? Description { get; set; }
    public string? MinimumSpecs { get; set; }
    public string? MaximumSpecs { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Check sheet definition used for Department / Section / Name / Frequency cascading filters.</summary>
public sealed class CheckSheetDefinition : AuditableEntity
{
    public string DepartmentCode { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
    public string SectionCode { get; set; } = default!;
    public string SectionName { get; set; } = default!;
    public string MachineName { get; set; } = default!;
    public string Frequency { get; set; } = default!;
    public string? DocumentControlNo { get; set; }
    public string? CheckSheetDisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<CheckSheetTemplateItem> TemplateItems { get; set; } = new List<CheckSheetTemplateItem>();
}

/// <summary>Template rows shown in Items Sorting for a given check sheet definition.</summary>
public sealed class CheckSheetTemplateItem : AuditableEntity
{
    public Guid CheckSheetDefinitionId { get; set; }
    public CheckSheetDefinition CheckSheetDefinition { get; set; } = default!;
    public string Items { get; set; } = default!;
    public string? Standard { get; set; }
    public string CheckPoint { get; set; } = default!;
    public string? Abnormality { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Items Sorting / CheckSheet Data line — one inspection result for a check point on a date.
/// Image1 maps from the legacy single Image column for backward compatibility.
/// </summary>
public sealed class CheckSheetData : AuditableEntity
{
    public string DepartmentCode { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
    public string SectionCode { get; set; } = default!;
    public string SectionName { get; set; } = default!;
    public string MachineName { get; set; } = default!;
    public string Frequency { get; set; } = default!;
    public DateTime CheckingDate { get; set; }
    public string Items { get; set; } = default!;
    public string? Standard { get; set; }
    public string CheckPoint { get; set; } = default!;
    public string? Abnormality { get; set; }
    public string? BeforeRemarks { get; set; }
    /// <summary>Snapshot of Check Point Master minimum specs at save time (reference).</summary>
    public string? SpecsMinimum { get; set; }
    /// <summary>Snapshot of Check Point Master maximum specs at save time (reference).</summary>
    public string? SpecsMaximum { get; set; }
    public string? ActualSpecs { get; set; }
    public string? Remarks { get; set; }
    /// <summary>Legacy single image — kept for existing records; also used as Image1.</summary>
    public string? Image { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public string? Image3 { get; set; }
    public string? Image4 { get; set; }
    public string? Priority { get; set; }
    /// <summary>Action status: "3", "1", or "0" (existing behaviour).</summary>
    public string? Status { get; set; }
    /// <summary>Score dropdown value 0–10.</summary>
    public int? Score { get; set; }
    public string? AuditCategory { get; set; }
    public bool Ok { get; set; }
    public bool Ng { get; set; }
    public bool Change { get; set; }
    public string? CreatedByName { get; set; }
}
