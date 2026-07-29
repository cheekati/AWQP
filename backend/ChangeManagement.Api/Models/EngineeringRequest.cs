namespace ChangeManagement.Api.Models;

public class EngineeringRequest
{
    public int Id { get; set; }

    /// <summary>Auto-generated base number, e.g. 2607001</summary>
    public string ErNumber { get; set; } = string.Empty;

    /// <summary>Current submission attempt (1 or 2). Full display: ErNumber-SubmissionNumber</summary>
    public int SubmissionNumber { get; set; } = 1;

    public string DisplayErNumber => $"{ErNumber}-{SubmissionNumber}";

    public Division Division { get; set; }
    public DateTime ValidationDate { get; set; }
    public DateTime SubmissionValidUntil { get; set; }
    public DateTime? ResubmitDeadline { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;

    // Reason for change (multi-select)
    public bool ReasonCostDown { get; set; }
    public bool ReasonAlternativeSourcing { get; set; }
    public bool ReasonOthers { get; set; }
    public string? ReasonOthersText { get; set; }

    public string? PresentDetails { get; set; }
    public string? NewDetails { get; set; }
    public string? Merit { get; set; }
    public string? Demerit { get; set; }
    public string? MaterialDisposition { get; set; }

    public int? SampleQuantity { get; set; }

    public TestLotType? TestLotType { get; set; }
    public string? TestLotDescription { get; set; }
    public string? TestLotCodeSerial { get; set; }

    public bool ApplicableToChemicalOrMaterials { get; set; }
    public string? SafetyDataSheet { get; set; }
    public string? ChemicalLabel { get; set; }
    public string? ChemicalClassification { get; set; }
    public string? ChemicalInventorySystem { get; set; }

    public int? VerifiedBySafetyUserId { get; set; }
    public int? CheckedByDeptHeadUserId { get; set; }
    public int? CheckedByQaUserId { get; set; }

    public VerificationDecision SafetyDecision { get; set; } = VerificationDecision.Pending;
    public VerificationDecision DeptHeadDecision { get; set; } = VerificationDecision.Pending;
    public VerificationDecision QaDecision { get; set; } = VerificationDecision.Pending;
    public VerificationDecision CooDecision { get; set; } = VerificationDecision.Pending;

    public string? SafetyComment { get; set; }
    public string? DeptHeadComment { get; set; }
    public string? QaComment { get; set; }
    public string? CooComment { get; set; }

    public DateTime? SafetyDecidedAt { get; set; }
    public DateTime? DeptHeadDecidedAt { get; set; }
    public DateTime? QaDecidedAt { get; set; }
    public DateTime? CooDecidedAt { get; set; }

    public int? ApprovedByCooUserId { get; set; }

    public ErStatus Status { get; set; } = ErStatus.Draft;
    public ErOutcome Outcome { get; set; } = ErOutcome.None;

    public int RequesterId { get; set; }
    public User? Requester { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    public ICollection<ErAttachment> Attachments { get; set; } = new List<ErAttachment>();
    public ICollection<ErComment> Comments { get; set; } = new List<ErComment>();
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
