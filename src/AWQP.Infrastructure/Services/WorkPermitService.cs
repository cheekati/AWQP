using AWQP.Application.Common;
using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Services;

public sealed class WorkPermitService : IWorkPermitService
{
    private static readonly string[] ProgressStatuses =
    [
        "APPROVED", "IN PROGRESS", "ON HOLD", "COMPLETED"
    ];

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public WorkPermitService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AtmosphericParameterRangeDto>> GetAcceptableRangesAsync(CancellationToken cancellationToken = default)
    {
        var ranges = await _unitOfWork.Repository<AtmosphericParameterRange>().Query()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new AtmosphericParameterRangeDto(
                x.ParameterCode, x.ParameterName, x.Unit, x.MinAcceptable, x.MaxAcceptable, x.DisplayRange, x.SortOrder))
            .ToListAsync(cancellationToken);

        if (ranges.Count > 0)
        {
            return ranges;
        }

        return DefaultRanges();
    }

    public async Task<ProgressWorkPermitFrontPageDto> GetProgressFrontPageAsync(string supplierCode, CancellationToken cancellationToken = default)
    {
        var ranges = await GetAcceptableRangesAsync(cancellationToken);
        var code = (supplierCode ?? string.Empty).Trim();

        var permits = await _unitOfWork.Repository<WorkPermit>().Query()
            .Include(x => x.DailyAtmosphericReadings)
            .Where(x => x.IsActive && x.SupplierCode == code && ProgressStatuses.Contains(x.Status))
            .OrderByDescending(x => x.PermitNumber)
            .ToListAsync(cancellationToken);

        var dtos = permits.Select(p => MapPermit(p, ranges)).ToList();
        return new ProgressWorkPermitFrontPageDto(ranges, dtos);
    }

    public async Task<Result<ProgressWorkPermitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ranges = await GetAcceptableRangesAsync(cancellationToken);
        var permit = await _unitOfWork.Repository<WorkPermit>().Query()
            .Include(x => x.DailyAtmosphericReadings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return permit is null
            ? Result<ProgressWorkPermitDto>.Failure("Work permit not found.")
            : Result<ProgressWorkPermitDto>.Success(MapPermit(permit, ranges));
    }

    public async Task<Result<ProgressWorkPermitDto>> UpdateProgressAsync(Guid id, UpdateProgressWorkPermitRequest request, CancellationToken cancellationToken = default)
    {
        var ranges = await GetAcceptableRangesAsync(cancellationToken);
        var permit = await _unitOfWork.Repository<WorkPermit>().Query()
            .Include(x => x.DailyAtmosphericReadings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (permit is null)
        {
            return Result<ProgressWorkPermitDto>.Failure("Work permit not found.");
        }

        var status = (request.Status ?? string.Empty).Trim().ToUpperInvariant();
        if (status is not ("IN PROGRESS" or "ON HOLD" or "COMPLETED"))
        {
            return Result<ProgressWorkPermitDto>.Failure("Status must be IN PROGRESS, ON HOLD, or COMPLETED.");
        }

        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            return Result<ProgressWorkPermitDto>.Failure("Please enter invoice number.");
        }

        if (string.IsNullOrWhiteSpace(request.PoNumber))
        {
            return Result<ProgressWorkPermitDto>.Failure("Please select PO number.");
        }

        if (request.PoCost is null)
        {
            return Result<ProgressWorkPermitDto>.Failure("Please select PO cost.");
        }

        if (status == "COMPLETED")
        {
            if (string.IsNullOrWhiteSpace(request.SupportingDocument6) && string.IsNullOrWhiteSpace(permit.SupportingDocument6))
            {
                return Result<ProgressWorkPermitDto>.Failure("Please upload at least one image for work completion.");
            }
            if (string.IsNullOrWhiteSpace(request.UploadDocument1) && string.IsNullOrWhiteSpace(permit.UploadDocument1))
            {
                return Result<ProgressWorkPermitDto>.Failure("Please upload invoice.");
            }
        }

        var user = _currentUserService.UserName ?? "supplier";
        permit.Status = status;
        permit.InvoiceNumber = request.InvoiceNumber.Trim();
        permit.PoNumber = request.PoNumber.Trim();
        permit.PoCost = request.PoCost;
        permit.Po = true;
        permit.Pr = false;
        permit.NoPoPr = false;
        permit.InProgressRemarks = request.InProgressRemarks;
        permit.OnHoldRemarks = request.OnHoldRemarks;
        permit.CompletedRemarks = request.CompletedRemarks;
        if (!string.IsNullOrWhiteSpace(request.SupportingDocument6))
        {
            permit.SupportingDocument6 = request.SupportingDocument6;
        }
        if (!string.IsNullOrWhiteSpace(request.UploadDocument1))
        {
            permit.UploadDocument1 = request.UploadDocument1;
        }

        if (status == "IN PROGRESS")
        {
            permit.InProgressBy = user;
            permit.InProgressOn = DateTime.UtcNow;
        }
        else if (status == "ON HOLD")
        {
            permit.OnHoldBy = user;
            permit.OnHoldOn = DateTime.UtcNow;
        }
        else if (status == "COMPLETED")
        {
            permit.CompletedBy = user;
            permit.CompletedOn = DateTime.UtcNow;
        }

        _unitOfWork.Repository<WorkPermit>().Update(permit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProgressWorkPermitDto>.Success(MapPermit(permit, ranges));
    }

    public async Task<Result<DailyAtmosphericReadingDto>> AddDailyReadingAsync(Guid workPermitId, AddDailyAtmosphericReadingRequest request, CancellationToken cancellationToken = default)
    {
        var ranges = await GetAcceptableRangesAsync(cancellationToken);
        var permit = await _unitOfWork.Repository<WorkPermit>().GetByIdAsync(workPermitId, cancellationToken);
        if (permit is null)
        {
            return Result<DailyAtmosphericReadingDto>.Failure("Work permit not found.");
        }

        var exists = await _unitOfWork.Repository<DailyAtmosphericReading>().Query()
            .AnyAsync(x => x.WorkPermitId == workPermitId && x.ReadingDate == request.ReadingDate, cancellationToken);
        if (exists)
        {
            return Result<DailyAtmosphericReadingDto>.Failure($"A reading for {request.ReadingDate:dd-MMM-yyyy} already exists.");
        }

        var reading = new DailyAtmosphericReading
        {
            WorkPermitId = workPermitId,
            ReadingDate = request.ReadingDate,
            ReadingDateTimeUtc = DateTime.UtcNow,
            OxygenContentPercent = request.OxygenContentPercent,
            ToxicGasH2SPpm = request.ToxicGasH2SPpm,
            CarbonMonoxidePpm = request.CarbonMonoxidePpm,
            CombustibleGasLelPercent = request.CombustibleGasLelPercent,
            PicName = request.PicName,
            Remarks = request.Remarks
        };
        await _unitOfWork.Repository<DailyAtmosphericReading>().AddAsync(reading, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<DailyAtmosphericReadingDto>.Success(MapReading(reading, ranges));
    }

    private static ProgressWorkPermitDto MapPermit(WorkPermit p, IReadOnlyList<AtmosphericParameterRangeDto> ranges) =>
        new(
            p.Id,
            p.PermitNumber,
            p.WorkType,
            p.Status,
            p.Department,
            p.DepartmentName,
            p.Section,
            p.SectionName,
            p.PlantLocation,
            p.WorkInfo,
            p.DetailDescription,
            p.WorkScheduleDateFrom,
            p.WorkScheduleDateTo,
            p.SupplierCode,
            p.SupplierName,
            p.PoNumber,
            p.PoCost,
            p.InvoiceNumber,
            p.NoPoPr,
            p.Po,
            p.Pr,
            p.SupportingDocument6,
            p.InProgressRemarks,
            p.OnHoldRemarks,
            p.CompletedRemarks,
            p.DailyAtmosphericReadings
                .OrderBy(r => r.ReadingDate)
                .Select(r => MapReading(r, ranges))
                .ToList());

    private static DailyAtmosphericReadingDto MapReading(DailyAtmosphericReading r, IReadOnlyList<AtmosphericParameterRangeDto> ranges)
    {
        var o2 = IsInRange(r.OxygenContentPercent, ranges, "O2");
        var h2s = IsInRange(r.ToxicGasH2SPpm, ranges, "H2S");
        var co = IsInRange(r.CarbonMonoxidePpm, ranges, "CO");
        var lel = IsInRange(r.CombustibleGasLelPercent, ranges, "LEL");
        return new DailyAtmosphericReadingDto(
            r.Id,
            r.ReadingDate,
            r.ReadingDateTimeUtc,
            r.OxygenContentPercent,
            r.ToxicGasH2SPpm,
            r.CarbonMonoxidePpm,
            r.CombustibleGasLelPercent,
            r.PicName,
            r.Remarks,
            o2, h2s, co, lel,
            o2 && h2s && co && lel);
    }

    private static bool IsInRange(decimal value, IReadOnlyList<AtmosphericParameterRangeDto> ranges, string code)
    {
        var range = ranges.FirstOrDefault(x => x.ParameterCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (range is null)
        {
            return true;
        }
        if (range.MinAcceptable is not null && value < range.MinAcceptable.Value)
        {
            return false;
        }
        if (range.MaxAcceptable is not null && value > range.MaxAcceptable.Value)
        {
            return false;
        }
        return true;
    }

    private static IReadOnlyList<AtmosphericParameterRangeDto> DefaultRanges() =>
    [
        new("O2", "Oxygen Content", "%", 19.50m, 23.50m, "19.5 – 23.5%", 1),
        new("H2S", "Toxic Gas H2S", "ppm", 0m, 10m, "≤ 10 ppm", 2),
        new("CO", "Carbon Monoxide", "ppm", 0m, 25m, "≤ 25 ppm", 3),
        new("LEL", "Combustible Gas", "%LEL", 0m, 10m, "≤ 10% LEL", 4)
    ];
}
