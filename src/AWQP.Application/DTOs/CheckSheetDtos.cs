namespace AWQP.Application.DTOs;

// --- Check Point Master ---

public sealed record CheckPointMasterDto(
    Guid Id,
    string CheckPointName,
    string? Description,
    string? MinimumSpecs,
    string? MaximumSpecs,
    bool IsActive,
    bool HasSpecs);

public sealed record CreateCheckPointMasterRequest(
    string CheckPointName,
    string? Description,
    string? MinimumSpecs,
    string? MaximumSpecs,
    bool IsActive = true);

public sealed record UpdateCheckPointMasterRequest(
    string CheckPointName,
    string? Description,
    string? MinimumSpecs,
    string? MaximumSpecs,
    bool IsActive);

public sealed record UpdateCheckPointSpecsRequest(
    string? MinimumSpecs,
    string? MaximumSpecs);

// --- Check Sheet cascading lookups ---

public sealed record CheckSheetLookupDto(string Code, string Name);

public sealed record CheckSheetDefinitionDto(
    Guid Id,
    string DepartmentCode,
    string DepartmentName,
    string SectionCode,
    string SectionName,
    string MachineName,
    string Frequency,
    string? DocumentControlNo,
    string? CheckSheetDisplayName);

// --- Items Sorting grid ---

public sealed record CheckSheetGridRowDto(
    Guid? Id,
    int SerialNo,
    string Items,
    string? Standard,
    string CheckPoint,
    string? Abnormality,
    string? BeforeRemarks,
    string? SpecsMinimum,
    string? SpecsMaximum,
    string? ActualSpecs,
    bool ActualSpecsEnabled,
    string? Remarks,
    string? Image,
    string? Image1,
    string? Image2,
    string? Image3,
    string? Image4,
    string? Priority,
    string? Status,
    int? Score);

public sealed record SaveCheckSheetRowRequest(
    string DepartmentCode,
    string DepartmentName,
    string SectionCode,
    string SectionName,
    string MachineName,
    string Frequency,
    DateTime CheckingDate,
    string Items,
    string? Standard,
    string CheckPoint,
    string? Abnormality,
    string? BeforeRemarks,
    string? ActualSpecs,
    string? Remarks,
    string? Image,
    string? Image1,
    string? Image2,
    string? Image3,
    string? Image4,
    string? Priority,
    string? Status,
    int? Score,
    string? CreatedByName);

public sealed record CheckSheetUploadResult(string Path, int Slot);
