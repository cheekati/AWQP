using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Application.Services;

public class CooApprovalService : ICooApprovalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IVerificationService _verificationService;
    private readonly IEngineeringRequestService _erService;

    public CooApprovalService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IVerificationService verificationService,
        IEngineeringRequestService erService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _verificationService = verificationService;
        _erService = erService;
    }

    public async Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetPendingAsync(
        EngineeringRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        query.Status = ErStatus.AwaitingCooApproval;
        return await _verificationService.GetAssignedRequestsAsync(VerificationStage.COO, query, cancellationToken);
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> ActAsync(
        Guid erId,
        CooActionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Unauthorized.");

        if (!_currentUser.IsInRole(AppRoles.COO) && !_currentUser.IsInRole(AppRoles.Administrator))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Only COO can perform this action.");

        var erRepo = _unitOfWork.Repository<EngineeringRequest>();
        var entity = await erRepo.Query()
            .Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == erId && !x.IsDeleted, cancellationToken);

        if (entity is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Engineering request not found.");

        if (entity.Status != ErStatus.AwaitingCooApproval)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Request is not awaiting COO approval.");

        var assignmentRepo = _unitOfWork.Repository<VerificationAssignment>();
        var assignment = await assignmentRepo.Query()
            .FirstOrDefaultAsync(a => a.EngineeringRequestId == erId && a.Stage == VerificationStage.COO && !a.IsCompleted && !a.IsDeleted, cancellationToken);

        await _unitOfWork.Repository<ApprovalHistory>().AddAsync(new ApprovalHistory
        {
            EngineeringRequestId = erId,
            ApproverId = _currentUser.UserId.Value,
            Action = request.Action,
            Comments = request.Comments,
            ActionDate = DateTime.UtcNow,
            CreatedBy = _currentUser.UserName
        }, cancellationToken);

        if (assignment is not null)
        {
            assignment.IsCompleted = true;
            assignment.Result = request.Action;
            assignment.CompletedDate = DateTime.UtcNow;
            assignment.AssignedToUserId = _currentUser.UserId;
            assignment.UpdatedBy = _currentUser.UserName;
            assignment.UpdatedDate = DateTime.UtcNow;
            assignmentRepo.Update(assignment);
        }

        switch (request.Action)
        {
            case VerificationAction.Approved:
                entity.Status = ErStatus.Approved;
                entity.ClosedDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _notificationService.NotifyAsync(
                    entity.RequesterId,
                    NotificationType.FinalApproval,
                    $"ER {entity.ErNumber} finally approved",
                    $"Your Engineering Request '{entity.Title}' has been approved by the COO.",
                    entity.Id,
                    true,
                    cancellationToken);
                break;

            case VerificationAction.Rejected:
                entity.Status = ErStatus.Rejected;
                entity.LatestRejectionComments = request.Comments;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _notificationService.NotifyAsync(
                    entity.RequesterId,
                    NotificationType.Rejection,
                    $"ER {entity.ErNumber} rejected by COO",
                    $"Your Engineering Request was rejected by the COO. Comments: {request.Comments}",
                    entity.Id,
                    true,
                    cancellationToken);
                break;

            case VerificationAction.Resubmit:
                entity.Status = ErStatus.Resubmitted;
                entity.LatestRejectionComments = request.Comments;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _notificationService.NotifyAsync(
                    entity.RequesterId,
                    NotificationType.Resubmission,
                    $"ER {entity.ErNumber} returned for resubmission",
                    $"The COO requested resubmission. Comments: {request.Comments}",
                    entity.Id,
                    true,
                    cancellationToken);
                break;

            default:
                return ApiResponse<EngineeringRequestDetailDto>.Fail("Invalid COO action.");
        }

        entity.UpdatedBy = _currentUser.UserName;
        entity.UpdatedDate = DateTime.UtcNow;
        erRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _erService.GetByIdAsync(erId, cancellationToken);
    }
}
