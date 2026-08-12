using System.Globalization;
using System.Text.RegularExpressions;
using AWQP.Application.Common;
using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Services;

public sealed class CheckSheetService : ICheckSheetService
{
    private static readonly string DefaultImage = "images/CommonImage/noimage.png";
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CheckSheetService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<CheckPointMasterDto>> ListCheckPointsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.Repository<CheckPointMaster>().Query()
            .OrderBy(x => x.CheckPointName)
            .ToListAsync(cancellationToken);
        return list.Select(MapCheckPoint).ToList();
    }

    public async Task<Result<CheckPointMasterDto>> GetCheckPointAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<CheckPointMaster>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null
            ? Result<CheckPointMasterDto>.Failure("Check point not found.")
            : Result<CheckPointMasterDto>.Success(MapCheckPoint(entity));
    }

    public async Task<Result<CheckPointMasterDto>> CreateCheckPointAsync(CreateCheckPointMasterRequest request, CancellationToken cancellationToken = default)
    {
        var name = (request.CheckPointName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result<CheckPointMasterDto>.Failure("Check Point name is required.");

        var exists = await _unitOfWork.Repository<CheckPointMaster>().Query()
            .AnyAsync(x => x.CheckPointName == name, cancellationToken);
        if (exists)
            return Result<CheckPointMasterDto>.Failure($"Check Point '{name}' already exists.");

        var entity = new CheckPointMaster
        {
            CheckPointName = name,
            Description = NullIfEmpty(request.Description),
            MinimumSpecs = NullIfEmpty(request.MinimumSpecs),
            MaximumSpecs = NullIfEmpty(request.MaximumSpecs),
            IsActive = request.IsActive
        };
        await _unitOfWork.Repository<CheckPointMaster>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CheckPointMasterDto>.Success(MapCheckPoint(entity));
    }

    public async Task<Result<CheckPointMasterDto>> UpdateCheckPointAsync(Guid id, UpdateCheckPointMasterRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<CheckPointMaster>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result<CheckPointMasterDto>.Failure("Check point not found.");

        var name = (request.CheckPointName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result<CheckPointMasterDto>.Failure("Check Point name is required.");

        var duplicate = await _unitOfWork.Repository<CheckPointMaster>().Query()
            .AnyAsync(x => x.CheckPointName == name && x.Id != id, cancellationToken);
        if (duplicate)
            return Result<CheckPointMasterDto>.Failure($"Check Point '{name}' already exists.");

        entity.CheckPointName = name;
        entity.Description = NullIfEmpty(request.Description);
        entity.MinimumSpecs = NullIfEmpty(request.MinimumSpecs);
        entity.MaximumSpecs = NullIfEmpty(request.MaximumSpecs);
        entity.IsActive = request.IsActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CheckPointMasterDto>.Success(MapCheckPoint(entity));
    }

    public async Task<Result<CheckPointMasterDto>> UpdateCheckPointSpecsAsync(Guid id, UpdateCheckPointSpecsRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<CheckPointMaster>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result<CheckPointMasterDto>.Failure("Check point not found.");

        entity.MinimumSpecs = NullIfEmpty(request.MinimumSpecs);
        entity.MaximumSpecs = NullIfEmpty(request.MaximumSpecs);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CheckPointMasterDto>.Success(MapCheckPoint(entity));
    }

    public async Task<Result<bool>> DeleteCheckPointAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<CheckPointMaster>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result<bool>.Failure("Check point not found.");

        var inUse = await _unitOfWork.Repository<CheckSheetData>().Query()
            .AnyAsync(x => x.CheckPoint == entity.CheckPointName, cancellationToken);
        if (inUse)
            return Result<bool>.Failure("Cannot delete: this check point is used in Items Sorting records. Deactivate it instead.");

        var inTemplate = await _unitOfWork.Repository<CheckSheetTemplateItem>().Query()
            .AnyAsync(x => x.CheckPoint == entity.CheckPointName, cancellationToken);
        if (inTemplate)
            return Result<bool>.Failure("Cannot delete: this check point is used in check sheet templates. Deactivate it instead.");

        entity.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<IReadOnlyCollection<CheckSheetLookupDto>> ListDepartmentsAsync(CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .Where(x => x.IsActive)
            .GroupBy(x => new { x.DepartmentCode, x.DepartmentName })
            .Select(g => new CheckSheetLookupDto(g.Key.DepartmentCode, g.Key.DepartmentName))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<CheckSheetLookupDto>> ListSectionsAsync(string departmentCode, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .Where(x => x.IsActive && x.DepartmentCode == departmentCode)
            .GroupBy(x => new { x.SectionCode, x.SectionName })
            .Select(g => new CheckSheetLookupDto(g.Key.SectionCode, g.Key.SectionName))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> ListMachineNamesAsync(string departmentCode, string sectionCode, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .Where(x => x.IsActive && x.DepartmentCode == departmentCode && x.SectionCode == sectionCode)
            .Select(x => x.MachineName)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> ListFrequenciesAsync(string departmentCode, string sectionCode, string machineName, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .Where(x => x.IsActive && x.DepartmentCode == departmentCode && x.SectionCode == sectionCode && x.MachineName == machineName)
            .Select(x => x.Frequency)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public async Task<Result<CheckSheetDefinitionDto>> GetDefinitionAsync(string departmentCode, string sectionCode, string machineName, string frequency, CancellationToken cancellationToken = default)
    {
        var def = await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .FirstOrDefaultAsync(x =>
                x.IsActive &&
                x.DepartmentCode == departmentCode &&
                x.SectionCode == sectionCode &&
                x.MachineName == machineName &&
                x.Frequency == frequency, cancellationToken);
        return def is null
            ? Result<CheckSheetDefinitionDto>.Failure("Check sheet definition not found.")
            : Result<CheckSheetDefinitionDto>.Success(new CheckSheetDefinitionDto(
                def.Id, def.DepartmentCode, def.DepartmentName, def.SectionCode, def.SectionName,
                def.MachineName, def.Frequency, def.DocumentControlNo, def.CheckSheetDisplayName));
    }

    public async Task<IReadOnlyCollection<CheckSheetGridRowDto>> GetGridAsync(
        string departmentCode,
        string sectionCode,
        string machineName,
        string frequency,
        DateTime checkingDate,
        CancellationToken cancellationToken = default)
    {
        var date = checkingDate.Date;
        var definition = await _unitOfWork.Repository<CheckSheetDefinition>().Query()
            .FirstOrDefaultAsync(x =>
                x.IsActive &&
                x.DepartmentCode == departmentCode &&
                x.SectionCode == sectionCode &&
                x.MachineName == machineName &&
                x.Frequency == frequency, cancellationToken);

        if (definition is null)
            return Array.Empty<CheckSheetGridRowDto>();

        var templates = await _unitOfWork.Repository<CheckSheetTemplateItem>().Query()
            .Where(x => x.CheckSheetDefinitionId == definition.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var saved = await _unitOfWork.Repository<CheckSheetData>().Query()
            .Where(x =>
                x.DepartmentCode == departmentCode &&
                x.SectionCode == sectionCode &&
                x.MachineName == machineName &&
                x.Frequency == frequency &&
                x.CheckingDate == date)
            .ToListAsync(cancellationToken);

        var checkPoints = await _unitOfWork.Repository<CheckPointMaster>().Query()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        var specsByName = checkPoints.ToDictionary(x => x.CheckPointName, StringComparer.OrdinalIgnoreCase);

        var rows = new List<CheckSheetGridRowDto>();
        var serial = 1;
        foreach (var t in templates)
        {
            var existing = saved.FirstOrDefault(s =>
                string.Equals(s.Items, t.Items, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.CheckPoint, t.CheckPoint, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.Standard ?? string.Empty, t.Standard ?? string.Empty, StringComparison.OrdinalIgnoreCase));

            specsByName.TryGetValue(t.CheckPoint, out var master);
            var min = master?.MinimumSpecs;
            var max = master?.MaximumSpecs;
            var hasSpecs = HasConfiguredSpecs(min, max);

            var image1 = ResolveImage1(existing);
            rows.Add(new CheckSheetGridRowDto(
                existing?.Id,
                serial++,
                t.Items,
                t.Standard,
                t.CheckPoint,
                t.Abnormality,
                existing?.BeforeRemarks,
                min,
                max,
                existing?.ActualSpecs,
                hasSpecs,
                existing?.Remarks,
                image1,
                image1,
                existing?.Image2,
                existing?.Image3,
                existing?.Image4,
                existing?.Priority,
                existing?.Status,
                existing?.Score));
        }

        return rows;
    }

    public async Task<IReadOnlyCollection<DateTime>> GetCheckedDatesAsync(
        string departmentCode,
        string sectionCode,
        string machineName,
        string frequency,
        CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<CheckSheetData>().Query()
            .Where(x =>
                x.DepartmentCode == departmentCode &&
                x.SectionCode == sectionCode &&
                x.MachineName == machineName &&
                x.Frequency == frequency)
            .Select(x => x.CheckingDate)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public async Task<Result<CheckSheetGridRowDto>> SaveRowAsync(SaveCheckSheetRowRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Remarks))
            return Result<CheckSheetGridRowDto>.Failure("Please, enter remarks; if you don't have anything, type N/A");

        if (string.IsNullOrWhiteSpace(request.Priority))
            return Result<CheckSheetGridRowDto>.Failure("Please, select priority");

        if (request.Score is < 0 or > 10)
            return Result<CheckSheetGridRowDto>.Failure("Score must be between 0 and 10.");

        var master = await _unitOfWork.Repository<CheckPointMaster>().Query()
            .FirstOrDefaultAsync(x => x.IsActive && x.CheckPointName == request.CheckPoint, cancellationToken);

        var min = master?.MinimumSpecs;
        var max = master?.MaximumSpecs;
        var hasSpecs = HasConfiguredSpecs(min, max);

        if (hasSpecs)
        {
            if (string.IsNullOrWhiteSpace(request.ActualSpecs))
                return Result<CheckSheetGridRowDto>.Failure("Please enter Actual Specs for this check point.");

            var validation = ValidateActualSpecs(request.ActualSpecs!, min, max);
            if (validation is not null)
                return Result<CheckSheetGridRowDto>.Failure(validation);
        }
        else if (!string.IsNullOrWhiteSpace(request.ActualSpecs))
        {
            return Result<CheckSheetGridRowDto>.Failure("Actual Specs is disabled because this check point has no Minimum/Maximum Specs configured.");
        }

        var images = NormalizeImages(request);
        if ((request.Status == "1" || request.Status == "0") && IsEmptyImage(images.Image1))
            return Result<CheckSheetGridRowDto>.Failure("Please, Upload Image");

        var date = request.CheckingDate.Date;
        var repo = _unitOfWork.Repository<CheckSheetData>();
        var existing = await repo.Query().FirstOrDefaultAsync(x =>
            x.DepartmentCode == request.DepartmentCode &&
            x.SectionCode == request.SectionCode &&
            x.MachineName == request.MachineName &&
            x.Frequency == request.Frequency &&
            x.CheckingDate == date &&
            x.Items == request.Items &&
            x.CheckPoint == request.CheckPoint &&
            (x.Standard ?? string.Empty) == (request.Standard ?? string.Empty), cancellationToken);

        if (existing is null)
        {
            existing = new CheckSheetData
            {
                DepartmentCode = request.DepartmentCode,
                DepartmentName = request.DepartmentName,
                SectionCode = request.SectionCode,
                SectionName = request.SectionName,
                MachineName = request.MachineName,
                Frequency = request.Frequency,
                CheckingDate = date,
                Items = request.Items,
                Standard = request.Standard,
                CheckPoint = request.CheckPoint,
                Abnormality = request.Abnormality,
                CreatedByName = request.CreatedByName ?? _currentUserService.UserName
            };
            await repo.AddAsync(existing, cancellationToken);
        }

        existing.BeforeRemarks = NullIfEmpty(request.BeforeRemarks);
        existing.SpecsMinimum = min;
        existing.SpecsMaximum = max;
        existing.ActualSpecs = hasSpecs ? NullIfEmpty(request.ActualSpecs) : null;
        existing.Remarks = request.Remarks.Trim();
        existing.Image = images.Image1;
        existing.Image1 = images.Image1;
        existing.Image2 = images.Image2;
        existing.Image3 = images.Image3;
        existing.Image4 = images.Image4;
        existing.Priority = request.Priority;
        existing.Status = request.Status;
        existing.Score = request.Score;
        existing.Ok = request.Status == "3";
        existing.Ng = request.Status == "0";
        existing.Change = request.Status == "1";

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CheckSheetGridRowDto>.Success(new CheckSheetGridRowDto(
            existing.Id,
            0,
            existing.Items,
            existing.Standard,
            existing.CheckPoint,
            existing.Abnormality,
            existing.BeforeRemarks,
            existing.SpecsMinimum,
            existing.SpecsMaximum,
            existing.ActualSpecs,
            hasSpecs,
            existing.Remarks,
            existing.Image1,
            existing.Image1,
            existing.Image2,
            existing.Image3,
            existing.Image4,
            existing.Priority,
            existing.Status,
            existing.Score));
    }

    private static CheckPointMasterDto MapCheckPoint(CheckPointMaster x) =>
        new(x.Id, x.CheckPointName, x.Description, x.MinimumSpecs, x.MaximumSpecs, x.IsActive, HasConfiguredSpecs(x.MinimumSpecs, x.MaximumSpecs));

    private static bool HasConfiguredSpecs(string? min, string? max) =>
        !string.IsNullOrWhiteSpace(min) || !string.IsNullOrWhiteSpace(max);

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? ResolveImage1(CheckSheetData? existing)
    {
        if (existing is null) return null;
        if (!string.IsNullOrWhiteSpace(existing.Image1)) return existing.Image1;
        if (!string.IsNullOrWhiteSpace(existing.Image)) return existing.Image;
        return null;
    }

    private static bool IsEmptyImage(string? path) =>
        string.IsNullOrWhiteSpace(path) ||
        string.Equals(path, DefaultImage, StringComparison.OrdinalIgnoreCase);

    private static (string? Image1, string? Image2, string? Image3, string? Image4) NormalizeImages(SaveCheckSheetRowRequest request)
    {
        var image1 = NullIfEmpty(request.Image1) ?? NullIfEmpty(request.Image);
        var image2 = NullIfEmpty(request.Image2);
        var image3 = NullIfEmpty(request.Image3);
        var image4 = NullIfEmpty(request.Image4);

        var list = new List<string?>();
        foreach (var img in new[] { image1, image2, image3, image4 })
        {
            if (!IsEmptyImage(img) && list.Count < 4)
                list.Add(img);
        }

        return (
            list.ElementAtOrDefault(0),
            list.ElementAtOrDefault(1),
            list.ElementAtOrDefault(2),
            list.ElementAtOrDefault(3));
    }

    /// <summary>
    /// Validates Actual Specs against min/max when both sides can be parsed as numbers.
    /// Text-based specs (e.g. "OK", "Clear") skip numeric comparison.
    /// </summary>
    internal static string? ValidateActualSpecs(string actual, string? min, string? max)
    {
        var actualNum = TryParseLeadingNumber(actual);
        var minNum = TryParseLeadingNumber(min);
        var maxNum = TryParseLeadingNumber(max);

        if (actualNum is null)
        {
            // Allow non-numeric actual when specs are also non-numeric.
            if (minNum is null && maxNum is null)
                return null;
            return "Actual Specs must be a numeric value for this check point.";
        }

        if (minNum is not null && actualNum < minNum)
            return $"Actual Specs ({actualNum}) is below Minimum Specs ({min}).";

        if (maxNum is not null && actualNum > maxNum)
            return $"Actual Specs ({actualNum}) is above Maximum Specs ({max}).";

        return null;
    }

    private static decimal? TryParseLeadingNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = Regex.Match(value.Trim(), @"^-?\d+(\.\d+)?");
        if (!match.Success) return null;
        return decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
}
