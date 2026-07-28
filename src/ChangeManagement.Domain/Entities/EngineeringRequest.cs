using ChangeManagement.Domain.Common;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Domain.Entities;

public class EngineeringRequest : BaseEntity
{
    public string ErNumber { get; set; } = string.Empty;
    public Guid DivisionId { get; set; }
    public bool IsPr { get; set; }
    public bool IsEng { get; set; }
    public bool IsQa { get; set; }
    public bool IsInd { get; set; }
    public int SubmissionCount { get; set; } = 1;
    public DateTime ValidationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid ProductId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string? DetailsOfEvaluation { get; set; }
    public string? PresentCondition { get; set; }
    public string? NewCondition { get; set; }
    public string? Merit { get; set; }
    public string? Demerit { get; set; }
    public string? MaterialDisposition { get; set; }
    public int? SampleQuantity { get; set; }
    public TestLotIdentification? TestLotIdentification { get; set; }
    public string? TestLotDescription { get; set; }
    public bool ApplicableToChemicalOrMaterials { get; set; }
    public string? SafetyDataSheet { get; set; }
    public string? ChemicalLabel { get; set; }
    public string? ChemicalClassification { get; set; }
    public string? ChemicalInventoryManagementSystem { get; set; }
    public ErStatus Status { get; set; } = ErStatus.Draft;
    public Guid RequesterId { get; set; }
    public string? LatestRejectionComments { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public Division Division { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public User Requester { get; set; } = null!;
    public ICollection<EngineeringRequestReason> Reasons { get; set; } = new List<EngineeringRequestReason>();
    public ICollection<EngineeringRequestAttachment> Attachments { get; set; } = new List<EngineeringRequestAttachment>();
    public ICollection<VerificationHistory> VerificationHistories { get; set; } = new List<VerificationHistory>();
    public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
