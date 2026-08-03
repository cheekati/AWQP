namespace AWQP.Application.DTOs;

public sealed record AtmosphericParameterRangeDto(
    string ParameterCode,
    string ParameterName,
    string Unit,
    decimal? MinAcceptable,
    decimal? MaxAcceptable,
    string DisplayRange,
    int SortOrder);

public sealed record DailyAtmosphericReadingDto(
    Guid Id,
    DateOnly ReadingDate,
    DateTime ReadingDateTimeUtc,
    decimal OxygenContentPercent,
    decimal ToxicGasH2SPpm,
    decimal CarbonMonoxidePpm,
    decimal CombustibleGasLelPercent,
    string? PicName,
    string? Remarks,
    bool OxygenInRange,
    bool H2SInRange,
    bool CoInRange,
    bool CombustibleInRange,
    bool AllInRange);

public sealed record ProgressWorkPermitDto(
    Guid Id,
    long PermitNumber,
    string WorkType,
    string Status,
    string Department,
    string DepartmentName,
    string Section,
    string SectionName,
    string PlantLocation,
    string WorkInfo,
    string? DetailDescription,
    DateTime? WorkScheduleDateFrom,
    DateTime? WorkScheduleDateTo,
    string SupplierCode,
    string SupplierName,
    string? PoNumber,
    decimal? PoCost,
    string? InvoiceNumber,
    bool NoPoPr,
    bool Po,
    bool Pr,
    string? SupportingDocument6,
    string? InProgressRemarks,
    string? OnHoldRemarks,
    string? CompletedRemarks,
    IReadOnlyList<DailyAtmosphericReadingDto> DailyAtmosphericReadings);

public sealed record UpdateProgressWorkPermitRequest(
    string Status,
    string? InvoiceNumber,
    string? PoNumber,
    decimal? PoCost,
    string? InProgressRemarks,
    string? OnHoldRemarks,
    string? CompletedRemarks,
    string? SupportingDocument6,
    string? UploadDocument1);

public sealed record AddDailyAtmosphericReadingRequest(
    DateOnly ReadingDate,
    decimal OxygenContentPercent,
    decimal ToxicGasH2SPpm,
    decimal CarbonMonoxidePpm,
    decimal CombustibleGasLelPercent,
    string? PicName,
    string? Remarks);

public sealed record ProgressWorkPermitFrontPageDto(
    IReadOnlyList<AtmosphericParameterRangeDto> AcceptableRanges,
    IReadOnlyList<ProgressWorkPermitDto> WorkPermits);
