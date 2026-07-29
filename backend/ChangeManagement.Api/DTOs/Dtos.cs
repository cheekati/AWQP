using System.ComponentModel.DataAnnotations;
using ChangeManagement.Api.Models;

namespace ChangeManagement.Api.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, UserDto User);

public record UserDto(int Id, string Email, string FullName, string Role, string? Department);

public class CreateErRequest
{
    [Required] public Division Division { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [Required] public string Department { get; set; } = string.Empty;
    [Required] public string Product { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;

    public bool ReasonCostDown { get; set; }
    public bool ReasonAlternativeSourcing { get; set; }
    public bool ReasonOthers { get; set; }
    public string? ReasonOthersText { get; set; }

    public string? PresentDetails { get; set; }
    public string? NewDetails { get; set; }
    public string? Merit { get; set; }
    public string? Demerit { get; set; }
    public string? MaterialDisposition { get; set; }

    [Range(0, int.MaxValue)]
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

    public bool Submit { get; set; }
}

public class UpdateErRequest : CreateErRequest { }

public class VerificationActionRequest
{
    [Required] public VerificationDecision Decision { get; set; }
    public string? Comment { get; set; }
}

public class CooActionRequest
{
    [Required] public VerificationDecision Decision { get; set; }
    public string? Comment { get; set; }
    public ErOutcome? Outcome { get; set; }
}

public record AttachmentDto(
    int Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    bool IsImage,
    string Url,
    DateTime UploadedAt);

public record CommentDto(
    int Id,
    string UserName,
    string Action,
    string Comment,
    DateTime CreatedAt);

public record ErListItemDto(
    int Id,
    string ErNumber,
    int SubmissionNumber,
    string DisplayErNumber,
    string Title,
    string Division,
    string Department,
    string Product,
    string Status,
    string Outcome,
    string RequesterName,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime ValidationDate,
    DateTime SubmissionValidUntil,
    int DaysRemaining);

public record ErDetailDto(
    int Id,
    string ErNumber,
    int SubmissionNumber,
    string DisplayErNumber,
    string Division,
    DateTime ValidationDate,
    DateTime SubmissionValidUntil,
    DateTime? ResubmitDeadline,
    int DaysRemaining,
    string Title,
    string Department,
    string Product,
    string Customer,
    string Process,
    bool ReasonCostDown,
    bool ReasonAlternativeSourcing,
    bool ReasonOthers,
    string? ReasonOthersText,
    string? PresentDetails,
    string? NewDetails,
    string? Merit,
    string? Demerit,
    string? MaterialDisposition,
    int? SampleQuantity,
    string? TestLotType,
    string? TestLotDescription,
    string? TestLotCodeSerial,
    bool ApplicableToChemicalOrMaterials,
    string? SafetyDataSheet,
    string? ChemicalLabel,
    string? ChemicalClassification,
    string? ChemicalInventorySystem,
    int? VerifiedBySafetyUserId,
    int? CheckedByDeptHeadUserId,
    int? CheckedByQaUserId,
    string? VerifiedBySafetyName,
    string? CheckedByDeptHeadName,
    string? CheckedByQaName,
    string SafetyDecision,
    string DeptHeadDecision,
    string QaDecision,
    string CooDecision,
    string? SafetyComment,
    string? DeptHeadComment,
    string? QaComment,
    string? CooComment,
    string Status,
    string Outcome,
    int RequesterId,
    string RequesterName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<AttachmentDto> Attachments,
    IReadOnlyList<CommentDto> Comments,
    bool CanEdit,
    bool CanVerify,
    bool CanCooAct);

public record LookupDto(string Category, IReadOnlyList<string> Values);
