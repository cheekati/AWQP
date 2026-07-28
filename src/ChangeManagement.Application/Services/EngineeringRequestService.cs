using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Application.Services;

public class EngineeringRequestService : IEngineeringRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public EngineeringRequestService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> CreateAsync(CreateEngineeringRequestDto request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Unauthorized.");

        var erNumber = await GenerateErNumberAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var entity = new EngineeringRequest
        {
            ErNumber = erNumber,
            DivisionId = request.DivisionId,
            IsPr = request.IsPr,
            IsEng = request.IsEng,
            IsQa = request.IsQa,
            IsInd = request.IsInd,
            SubmissionCount = 1,
            ValidationDate = now,
            ExpiryDate = now.AddDays(89),
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            ProductId = request.ProductId,
            Customer = request.Customer,
            Process = request.Process,
            DetailsOfEvaluation = request.DetailsOfEvaluation,
            PresentCondition = request.PresentCondition,
            NewCondition = request.NewCondition,
            Merit = request.Merit,
            Demerit = request.Demerit,
            MaterialDisposition = request.MaterialDisposition,
            SampleQuantity = request.SampleQuantity,
            TestLotIdentification = request.TestLotIdentification,
            TestLotDescription = request.TestLotDescription,
            ApplicableToChemicalOrMaterials = request.ApplicableToChemicalOrMaterials,
            SafetyDataSheet = request.SafetyDataSheet,
            ChemicalLabel = request.ChemicalLabel,
            ChemicalClassification = request.ChemicalClassification,
            ChemicalInventoryManagementSystem = request.ChemicalInventoryManagementSystem,
            Status = ErStatus.Draft,
            RequesterId = _currentUser.UserId.Value,
            CreatedBy = _currentUser.UserName
        };

        ApplyReasons(entity, request.Reasons, request.OtherReasonDescription);

        await _unitOfWork.Repository<EngineeringRequest>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Submit)
        {
            return await SubmitAsync(entity.Id, cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> UpdateAsync(Guid id, UpdateEngineeringRequestDto request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EngineeringRequest>();
        var entity = await repo.Query()
            .Include(x => x.Reasons)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Engineering request not found.");

        if (entity.RequesterId != _currentUser.UserId && !_currentUser.IsInRole(AppRoles.Administrator))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Only the requester can edit this request.");

        if (entity.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.Resubmitted))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Request cannot be edited in its current status.");

        if (entity.Status is ErStatus.Rejected or ErStatus.Resubmitted)
        {
            if (entity.SubmissionCount >= 2)
                return ApiResponse<EngineeringRequestDetailDto>.Fail("Maximum of 2 submissions allowed.");
        }

        // ER Number never changes
        entity.DivisionId = request.DivisionId;
        entity.IsPr = request.IsPr;
        entity.IsEng = request.IsEng;
        entity.IsQa = request.IsQa;
        entity.IsInd = request.IsInd;
        entity.Title = request.Title;
        entity.DepartmentId = request.DepartmentId;
        entity.ProductId = request.ProductId;
        entity.Customer = request.Customer;
        entity.Process = request.Process;
        entity.DetailsOfEvaluation = request.DetailsOfEvaluation;
        entity.PresentCondition = request.PresentCondition;
        entity.NewCondition = request.NewCondition;
        entity.Merit = request.Merit;
        entity.Demerit = request.Demerit;
        entity.MaterialDisposition = request.MaterialDisposition;
        entity.SampleQuantity = request.SampleQuantity;
        entity.TestLotIdentification = request.TestLotIdentification;
        entity.TestLotDescription = request.TestLotDescription;
        entity.ApplicableToChemicalOrMaterials = request.ApplicableToChemicalOrMaterials;
        entity.SafetyDataSheet = request.SafetyDataSheet;
        entity.ChemicalLabel = request.ChemicalLabel;
        entity.ChemicalClassification = request.ChemicalClassification;
        entity.ChemicalInventoryManagementSystem = request.ChemicalInventoryManagementSystem;
        entity.UpdatedBy = _currentUser.UserName;
        entity.UpdatedDate = DateTime.UtcNow;

        entity.Reasons.Clear();
        ApplyReasons(entity, request.Reasons, request.OtherReasonDescription);

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Submit)
        {
            return await SubmitAsync(entity.Id, cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<EngineeringRequest>().Query()
            .Include(x => x.Division)
            .Include(x => x.Department)
            .Include(x => x.Product)
            .Include(x => x.Requester)
            .Include(x => x.Reasons)
            .Include(x => x.Attachments.Where(a => !a.IsDeleted))
            .Include(x => x.VerificationHistories).ThenInclude(v => v.Verifier)
            .Include(x => x.ApprovalHistories).ThenInclude(a => a.Approver)
            .Include(x => x.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Engineering request not found.");

        var dto = _mapper.Map<EngineeringRequestDetailDto>(entity);

        var assignments = await _unitOfWork.Repository<VerificationAssignment>().Query()
            .Include(a => a.AssignedToUser)
            .Where(a => a.EngineeringRequestId == id && !a.IsDeleted)
            .ToListAsync(cancellationToken);
        dto.Assignments = _mapper.Map<List<VerificationAssignmentDto>>(assignments);

        return ApiResponse<EngineeringRequestDetailDto>.Ok(dto);
    }

    public async Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetMyRequestsAsync(EngineeringRequestQuery query, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<PagedResult<EngineeringRequestListDto>>.Fail("Unauthorized.");

        var q = _unitOfWork.Repository<EngineeringRequest>().Query()
            .Include(x => x.Division)
            .Include(x => x.Department)
            .Include(x => x.Product)
            .Include(x => x.Requester)
            .Where(x => !x.IsDeleted && x.RequesterId == _currentUser.UserId);

        return ApiResponse<PagedResult<EngineeringRequestListDto>>.Ok(await PageAsync(q, query, cancellationToken));
    }

    public async Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetAllAsync(EngineeringRequestQuery query, CancellationToken cancellationToken = default)
    {
        var q = _unitOfWork.Repository<EngineeringRequest>().Query()
            .Include(x => x.Division)
            .Include(x => x.Department)
            .Include(x => x.Product)
            .Include(x => x.Requester)
            .Where(x => !x.IsDeleted);

        return ApiResponse<PagedResult<EngineeringRequestListDto>>.Ok(await PageAsync(q, query, cancellationToken));
    }

    public async Task<ApiResponse<EngineeringRequestDetailDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EngineeringRequest>();
        var entity = await repo.Query()
            .Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Engineering request not found.");

        if (entity.RequesterId != _currentUser.UserId && !_currentUser.IsInRole(AppRoles.Administrator))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Only the requester can submit this request.");

        if (entity.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.Resubmitted))
            return ApiResponse<EngineeringRequestDetailDto>.Fail("Request cannot be submitted in its current status.");

        if (entity.Status is ErStatus.Rejected or ErStatus.Resubmitted)
        {
            if (entity.SubmissionCount >= 2)
                return ApiResponse<EngineeringRequestDetailDto>.Fail("Maximum of 2 submissions allowed.");

            entity.SubmissionCount += 1;
            entity.ValidationDate = DateTime.UtcNow;
            entity.ExpiryDate = DateTime.UtcNow.AddDays(89);
            entity.Status = ErStatus.Resubmitted;
        }
        else
        {
            entity.Status = ErStatus.Submitted;
        }

        entity.SubmittedDate = DateTime.UtcNow;
        entity.LatestRejectionComments = null;
        entity.Status = ErStatus.InProgress;
        entity.UpdatedBy = _currentUser.UserName;
        entity.UpdatedDate = DateTime.UtcNow;

        // Reset prior incomplete assignments and create new ones for Safety, Dept Head, QA
        var assignmentRepo = _unitOfWork.Repository<VerificationAssignment>();
        var existing = await assignmentRepo.FindAsync(a => a.EngineeringRequestId == id && !a.IsCompleted, cancellationToken);
        foreach (var a in existing)
        {
            a.IsDeleted = true;
            assignmentRepo.Update(a);
        }

        foreach (var stage in new[] { VerificationStage.Safety, VerificationStage.DepartmentHead, VerificationStage.QA })
        {
            await assignmentRepo.AddAsync(new VerificationAssignment
            {
                EngineeringRequestId = entity.Id,
                Stage = stage,
                IsCompleted = false,
                CreatedBy = _currentUser.UserName
            }, cancellationToken);
        }

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var subject = $"Engineering Request {entity.ErNumber} submitted";
        var message = $"ER '{entity.Title}' ({entity.ErNumber}) has been submitted by {entity.Requester.FullName} and requires verification.";

        await _notificationService.NotifyRoleAsync(AppRoles.Safety, NotificationType.RequestSubmission, subject, message, entity.Id, true, cancellationToken);
        await _notificationService.NotifyRoleAsync(AppRoles.DepartmentHead, NotificationType.VerificationAssignment, subject, message, entity.Id, true, cancellationToken);
        await _notificationService.NotifyRoleAsync(AppRoles.QA, NotificationType.VerificationAssignment, subject, message, entity.Id, true, cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EngineeringRequest>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("Not found.");

        if (entity.RequesterId != _currentUser.UserId)
            return ApiResponse<bool>.Fail("Unauthorized.");

        if (entity.Status != ErStatus.Draft)
            return ApiResponse<bool>.Fail("Only draft requests can be deleted.");

        entity.IsDeleted = true;
        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserName;
        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Draft deleted.");
    }

    private static void ApplyReasons(EngineeringRequest entity, List<ReasonForChange> reasons, string? otherDescription)
    {
        foreach (var reason in reasons.Distinct())
        {
            entity.Reasons.Add(new EngineeringRequestReason
            {
                Reason = reason,
                OtherDescription = reason == ReasonForChange.Others ? otherDescription : null,
                CreatedBy = entity.CreatedBy
            });
        }
    }

    private async Task<string> GenerateErNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ER-{year}-";
        var last = await _unitOfWork.Repository<EngineeringRequest>().Query()
            .Where(x => x.ErNumber.StartsWith(prefix))
            .OrderByDescending(x => x.ErNumber)
            .Select(x => x.ErNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var next = 1;
        if (!string.IsNullOrEmpty(last))
        {
            var parts = last.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var n))
                next = n + 1;
        }

        return $"{prefix}{next:D5}";
    }

    private async Task<PagedResult<EngineeringRequestListDto>> PageAsync(IQueryable<EngineeringRequest> q, EngineeringRequestQuery query, CancellationToken cancellationToken)
    {
        if (query.Status.HasValue)
            q = q.Where(x => x.Status == query.Status);
        if (query.DivisionId.HasValue)
            q = q.Where(x => x.DivisionId == query.DivisionId);
        if (query.DepartmentId.HasValue)
            q = q.Where(x => x.DepartmentId == query.DepartmentId);
        if (query.ProductId.HasValue)
            q = q.Where(x => x.ProductId == query.ProductId);
        if (query.FromDate.HasValue)
            q = q.Where(x => x.CreatedDate >= query.FromDate);
        if (query.ToDate.HasValue)
            q = q.Where(x => x.CreatedDate <= query.ToDate);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(x => x.ErNumber.ToLower().Contains(s) || x.Title.ToLower().Contains(s) || x.Customer.ToLower().Contains(s));
        }

        q = (query.SortBy?.ToLower()) switch
        {
            "ernumber" => query.SortDescending ? q.OrderByDescending(x => x.ErNumber) : q.OrderBy(x => x.ErNumber),
            "title" => query.SortDescending ? q.OrderByDescending(x => x.Title) : q.OrderBy(x => x.Title),
            "status" => query.SortDescending ? q.OrderByDescending(x => x.Status) : q.OrderBy(x => x.Status),
            "submitteddate" => query.SortDescending ? q.OrderByDescending(x => x.SubmittedDate) : q.OrderBy(x => x.SubmittedDate),
            _ => query.SortDescending ? q.OrderByDescending(x => x.CreatedDate) : q.OrderBy(x => x.CreatedDate)
        };

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .ProjectTo<EngineeringRequestListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<EngineeringRequestListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
