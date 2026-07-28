using AutoMapper;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.Dashboard;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var q = _unitOfWork.Repository<EngineeringRequest>().Query().Where(x => !x.IsDeleted);

        if (_currentUser.IsInRole(AppRoles.Requester) &&
            !_currentUser.IsInRole(AppRoles.Administrator) &&
            !_currentUser.IsInRole(AppRoles.COO) &&
            !_currentUser.IsInRole(AppRoles.Safety) &&
            !_currentUser.IsInRole(AppRoles.DepartmentHead) &&
            !_currentUser.IsInRole(AppRoles.QA))
        {
            q = q.Where(x => x.RequesterId == _currentUser.UserId);
        }

        var all = await q.Select(x => new { x.Id, x.Status, x.CreatedDate, x.ErNumber, x.Title }).ToListAsync(cancellationToken);

        var awaiting = 0;
        if (_currentUser.IsInRole(AppRoles.Safety))
            awaiting += await CountPendingAsync(VerificationStage.Safety, cancellationToken);
        if (_currentUser.IsInRole(AppRoles.DepartmentHead))
            awaiting += await CountPendingAsync(VerificationStage.DepartmentHead, cancellationToken);
        if (_currentUser.IsInRole(AppRoles.QA))
            awaiting += await CountPendingAsync(VerificationStage.QA, cancellationToken);
        if (_currentUser.IsInRole(AppRoles.COO))
            awaiting += await CountPendingAsync(VerificationStage.COO, cancellationToken);

        var stats = new DashboardStatsDto
        {
            TotalRequests = all.Count,
            DraftCount = all.Count(x => x.Status == ErStatus.Draft),
            InProgressCount = all.Count(x => x.Status is ErStatus.InProgress or ErStatus.Submitted or ErStatus.AwaitingCooApproval),
            ApprovedCount = all.Count(x => x.Status == ErStatus.Approved),
            RejectedCount = all.Count(x => x.Status == ErStatus.Rejected),
            ClosedCount = all.Count(x => x.Status == ErStatus.Closed),
            AwaitingMyAction = awaiting,
            StatusBreakdown = Enum.GetValues<ErStatus>()
                .Select(s => new StatusCountDto { Status = s, StatusName = s.ToString(), Count = all.Count(x => x.Status == s) })
                .Where(x => x.Count > 0)
                .ToList(),
            MonthlyTrend = all
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .TakeLast(6)
                .Select(g => new MonthlyCountDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                }).ToList(),
            RecentRequests = all.OrderByDescending(x => x.CreatedDate).Take(8)
                .Select(x => new RecentRequestDto
                {
                    Id = x.Id,
                    ErNumber = x.ErNumber,
                    Title = x.Title,
                    Status = x.Status,
                    StatusName = x.Status.ToString(),
                    CreatedDate = x.CreatedDate
                }).ToList()
        };

        return ApiResponse<DashboardStatsDto>.Ok(stats);
    }

    private async Task<int> CountPendingAsync(VerificationStage stage, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<VerificationAssignment>().Query()
            .Where(a => a.Stage == stage && !a.IsCompleted && !a.IsDeleted)
            .CountAsync(cancellationToken);
    }
}

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public AttachmentService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<IReadOnlyList<AttachmentDto>>> UploadAsync(Guid erId, IFormFileCollection files, CancellationToken cancellationToken = default)
    {
        var er = await _unitOfWork.Repository<EngineeringRequest>().GetByIdAsync(erId, cancellationToken);
        if (er is null || er.IsDeleted)
            return ApiResponse<IReadOnlyList<AttachmentDto>>.Fail("Engineering request not found.");

        if (er.RequesterId != _currentUser.UserId && !_currentUser.IsInRole(AppRoles.Administrator))
            return ApiResponse<IReadOnlyList<AttachmentDto>>.Fail("Only the requester can upload attachments before final submission.");

        if (er.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.Resubmitted))
            return ApiResponse<IReadOnlyList<AttachmentDto>>.Fail("Attachments can only be modified before final submission while editable.");

        if (files is null || files.Count == 0)
            return ApiResponse<IReadOnlyList<AttachmentDto>>.Fail("No files provided.");

        var saved = new List<EngineeringRequestAttachment>();
        foreach (var file in files)
        {
            var (storedName, path, isImage) = await _fileStorage.SaveAsync(file, erId.ToString(), cancellationToken);
            var attachment = new EngineeringRequestAttachment
            {
                EngineeringRequestId = erId,
                FileName = storedName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                StoragePath = path,
                IsImage = isImage,
                CreatedBy = _currentUser.UserName
            };
            await _unitOfWork.Repository<EngineeringRequestAttachment>().AddAsync(attachment, cancellationToken);
            saved.Add(attachment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<AttachmentDto>>.Ok(_mapper.Map<List<AttachmentDto>>(saved));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid erId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var er = await _unitOfWork.Repository<EngineeringRequest>().GetByIdAsync(erId, cancellationToken);
        if (er is null)
            return ApiResponse<bool>.Fail("Not found.");

        if (er.RequesterId != _currentUser.UserId)
            return ApiResponse<bool>.Fail("Unauthorized.");

        if (er.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.Resubmitted))
            return ApiResponse<bool>.Fail("Cannot delete attachments after submission.");

        var attachment = await _unitOfWork.Repository<EngineeringRequestAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.EngineeringRequestId == erId && !a.IsDeleted, cancellationToken);
        if (attachment is null)
            return ApiResponse<bool>.Fail("Attachment not found.");

        attachment.IsDeleted = true;
        _unitOfWork.Repository<EngineeringRequestAttachment>().Update(attachment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _fileStorage.DeleteAsync(attachment.StoragePath, cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadAsync(Guid erId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _unitOfWork.Repository<EngineeringRequestAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.EngineeringRequestId == erId && !a.IsDeleted, cancellationToken);
        if (attachment is null) return null;

        var opened = await _fileStorage.OpenReadAsync(attachment.StoragePath, attachment.ContentType, cancellationToken);
        if (opened is null) return null;
        return (opened.Value.Stream, opened.Value.ContentType, attachment.OriginalFileName);
    }
}
