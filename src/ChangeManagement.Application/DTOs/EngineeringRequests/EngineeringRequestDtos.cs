using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Application.DTOs.EngineeringRequests;

public class CreateEngineeringRequestDto
{
    public Guid DivisionId { get; set; }
    public bool IsPr { get; set; }
    public bool IsEng { get; set; }
    public bool IsQa { get; set; }
    public bool IsInd { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid ProductId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string? DetailsOfEvaluation { get; set; }
    public List<ReasonForChange> Reasons { get; set; } = new();
    public string? OtherReasonDescription { get; set; }
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
    public bool Submit { get; set; }
}

public class UpdateEngineeringRequestDto : CreateEngineeringRequestDto
{
}

public class EngineeringRequestListDto
{
    public Guid Id { get; set; }
    public string ErNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public ErStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int SubmissionCount { get; set; }
    public DateTime ValidationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
}

public class EngineeringRequestDetailDto : EngineeringRequestListDto
{
    public Guid DivisionId { get; set; }
    public bool IsPr { get; set; }
    public bool IsEng { get; set; }
    public bool IsQa { get; set; }
    public bool IsInd { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ProductId { get; set; }
    public Guid RequesterId { get; set; }
    public string Process { get; set; } = string.Empty;
    public string? DetailsOfEvaluation { get; set; }
    public List<ReasonForChange> Reasons { get; set; } = new();
    public string? OtherReasonDescription { get; set; }
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
    public string? LatestRejectionComments { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<VerificationHistoryDto> VerificationHistories { get; set; } = new();
    public List<ApprovalHistoryDto> ApprovalHistories { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
    public List<VerificationAssignmentDto> Assignments { get; set; } = new();
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsImage { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class VerificationHistoryDto
{
    public Guid Id { get; set; }
    public VerificationStage Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public VerificationAction Action { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string VerifierName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
}

public class ApprovalHistoryDto
{
    public Guid Id { get; set; }
    public VerificationAction Action { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Stage { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class VerificationAssignmentDto
{
    public Guid Id { get; set; }
    public VerificationStage Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? AssignedToUserName { get; set; }
    public bool IsCompleted { get; set; }
    public VerificationAction? Result { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class EngineeringRequestQuery : ChangeManagement.Application.DTOs.Common.PagedQuery
{
    public ErStatus? Status { get; set; }
    public Guid? DivisionId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
