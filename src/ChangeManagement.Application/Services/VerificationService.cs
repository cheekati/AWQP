using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Application.Services;

public class VerificationService : IVerificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IEngineeringRequestService _erService;

    public VerificationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IEngineeringRequestService erService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _erService = erService;
    }

    public async Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetAssignedRequestsAsync(
        VerificationStage stage,
        EngineeringRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        var erIds = await _unitOfWork.Repository<VerificationAssignment>().Query()
            .Where(a => a.Stage == stage && !a.IsCompleted && !a.IsDeleted)
            .Select(a => a.EngineeringRequestId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var q = _unitOfWork.Repository<EngineeringRequest>().Query()
            .Include(x => x.Division)
            .Include(x => x.Department)
            .Include(x => x.Product)
            .Include(x => x.Requester)
            .Where(x => !x.IsDeleted && erIds.Contains(x.Id));

        if (query.Status.HasValue)
            q = q.Where(x => x.Status == query.Status);
        else if (stage == VerificationStage.COO)
            q = q.Where(x => x.Status == ErStatus.AwaitingCooApproval);
        else
            q = q.Where(x => x.Status == ErStatus.InProgress);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(x => x.ErNumber.ToLower().Contains(s) || x.Title.ToLower().Contains(s));
        }

        q = q.OrderByDescending(x => x.SubmittedDate);
        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .ProjectTo<EngineeringRequestListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<EngineeringRequestListDto>>.Ok(new PagedResult<EngineeringRequestListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> ActAsync(
        Guid erId,
        VerificationStage stage,
        VerificationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Unauthorized.");

        if (request.Action is not (VerificationAction.Approved or VerificationAction.Rejected or VerificationAction.Comment))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Invalid verification action.");

        var expectedRole = stage switch
        {
            VerificationStage.Safety => AppRoles.Safety,
            VerificationStage.DepartmentHead => AppRoles.DepartmentHead,
            VerificationStage.QA => AppRoles.QA,
            _ => null
        };

        if (expectedRole is null || (!_currentUser.IsInRole(expectedRole) && !_currentUser.IsInRole(AppRoles.Administrator)))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("You are not authorized for this verification stage.");

        var erRepo = _unitOfWork.Repository<EngineeringRequest>();
        var entity = await erRepo.Query()
            .Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == erId && !x.IsDeleted, cancellationToken);

        if (entity is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Engineering request not found.");

        if (entity.Status != ErStatus.InProgress)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Request is not in progress.");

        var assignmentRepo = _unitOfWork.Repository<VerificationAssignment>();
        var assignment = await assignmentRepo.Query()
            .FirstOrDefaultAsync(a => a.EngineeringRequestId == erId && a.Stage == stage && !a.IsCompleted && !a.IsDeleted, cancellationToken);

        if (assignment is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("No pending assignment for this stage.");

        if (request.Action == VerificationAction.Comment)
        {
            await _unitOfWork.Repository<Comment>().AddAsync(new Comment
            {
                EngineeringRequestId = erId,
                UserId = _currentUser.UserId.Value,
                Content = request.Comments ?? string.Empty,
                Stage = stage.ToString(),
                CreatedBy = _currentUser.UserName
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await _erService.GetByIdAsync(erId, cancellationToken);
        }

        await _unitOfWork.Repository<VerificationHistory>().AddAsync(new VerificationHistory
        {
            EngineeringRequestId = erId,
            VerifierId = _currentUser.UserId.Value,
            Stage = stage,
            Action = request.Action,
            Comments = request.Comments,
            ActionDate = DateTime.UtcNow,
            CreatedBy = _currentUser.UserName
        }, cancellationToken);

        assignment.IsCompleted = true;
        assignment.Result = request.Action;
        assignment.CompletedDate = DateTime.UtcNow;
        assignment.AssignedToUserId = _currentUser.UserId;
        assignment.UpdatedBy = _currentUser.UserName;
        assignment.UpdatedDate = DateTime.UtcNow;
        assignmentRepo.Update(assignment);

        if (request.Action == VerificationAction.Rejected)
        {
            entity.Status = ErStatus.Rejected;
            entity.LatestRejectionComments = request.Comments;
            entity.UpdatedBy = _currentUser.UserName;
            entity.UpdatedDate = DateTime.UtcNow;
            erRepo.Update(entity);

            // Cancel remaining assignments
            var pending = await assignmentRepo.FindAsync(a => a.EngineeringRequestId == erId && !a.IsCompleted && !a.IsDeleted, cancellationToken);
            foreach (var p in pending)
            {
                p.IsDeleted = true;
                assignmentRepo.Update(p);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyAsync(
                entity.RequesterId,
                NotificationType.Rejection,
                $"ER {entity.ErNumber} rejected",
                $"Your Engineering Request was rejected at {stage} stage. Comments: {request.Comments}",
                entity.Id,
                true,
                cancellationToken);

            return await _erService.GetByIdAsync(erId, cancellationToken);
        }

        // Approved
        entity.UpdatedBy = _currentUser.UserName;
        entity.UpdatedDate = DateTime.UtcNow;
        erRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            entity.RequesterId,
            NotificationType.Approval,
            $"ER {entity.ErNumber} verified by {stage}",
            $"Your Engineering Request was approved at {stage} stage.",
            entity.Id,
            true,
            cancellationToken);

        // Check if all three verifiers approved
        var stages = new[] { VerificationStage.Safety, VerificationStage.DepartmentHead, VerificationStage.QA };
        var completedApprovals = await assignmentRepo.Query()
            .Where(a => a.EngineeringRequestId == erId && !a.IsDeleted && a.IsCompleted && a.Result == VerificationAction.Approved)
            .Select(a => a.Stage)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (stages.All(s => completedApprovals.Contains(s)))
        {
            entity.Status = ErStatus.AwaitingCooApproval;
            erRepo.Update(entity);

            await assignmentRepo.AddAsync(new VerificationAssignment
            {
                EngineeringRequestId = entity.Id,
                Stage = VerificationStage.COO,
                IsCompleted = false,
                CreatedBy = _currentUser.UserName
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyRoleAsync(
                AppRoles.COO,
                NotificationType.VerificationAssignment,
                $"ER {entity.ErNumber} awaiting COO approval",
                $"All verifications complete. ER '{entity.Title}' requires COO approval.",
                entity.Id,
                true,
                cancellationToken);
        }

        return await _erService.GetByIdAsync(erId, cancellationToken);
    }

    public async Task<ApiResponse<CommentDto>> AddCommentAsync(Guid erId, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<CommentDto>.Fail("Unauthorized.");

        var exists = await _unitOfWork.Repository<EngineeringRequest>()
            .AnyAsync(x => x.Id == erId && !x.IsDeleted, cancellationToken);
        if (!exists)
            return ApiResponse<CommentDto>.Fail("Engineering request not found.");

        var comment = new Comment
        {
            EngineeringRequestId = erId,
            UserId = _currentUser.UserId.Value,
            Content = request.Content,
            CreatedBy = _currentUser.UserName
        };
        await _unitOfWork.Repository<Comment>().AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        return ApiResponse<CommentDto>.Ok(new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserName = user?.FullName ?? _currentUser.UserName ?? string.Empty,
            CreatedDate = comment.CreatedDate
        });
    }
}
