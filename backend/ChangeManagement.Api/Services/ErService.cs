using ChangeManagement.Api.Data;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IErService
{
    Task<ErDetailDto> CreateAsync(CreateErRequest request, User user);
    Task<ErDetailDto?> UpdateAsync(int id, UpdateErRequest request, User user);
    Task<ErDetailDto?> GetByIdAsync(int id, User user);
    Task<IReadOnlyList<ErListItemDto>> ListForUserAsync(User user);
    Task<IReadOnlyList<ErListItemDto>> ListVerificationQueueAsync(User user);
    Task<IReadOnlyList<ErListItemDto>> ListCooQueueAsync(User user);
    Task<ErDetailDto?> VerifyAsync(int id, VerificationActionRequest request, User user);
    Task<ErDetailDto?> CooActAsync(int id, CooActionRequest request, User user);
    Task<AttachmentDto?> AddAttachmentAsync(int erId, IFormFile file, User user);
    Task<bool> DeleteAttachmentAsync(int erId, int attachmentId, User user);
}

public class ErService : IErService
{
    public const int MaxSubmissions = 2;
    public const int SubmissionValidityMonths = 3;
    public const int ResubmitWindowDays = 89;

    private readonly AppDbContext _db;
    private readonly IErNumberService _erNumbers;
    private readonly IEmailService _email;
    private readonly IFileStorageService _files;

    public ErService(
        AppDbContext db,
        IErNumberService erNumbers,
        IEmailService email,
        IFileStorageService files)
    {
        _db = db;
        _erNumbers = erNumbers;
        _email = email;
        _files = files;
    }

    public async Task<ErDetailDto> CreateAsync(CreateErRequest request, User user)
    {
        ValidateRequest(request);

        var (erNumber, validationDate) = await _erNumbers.GenerateNextAsync();
        var er = new EngineeringRequest
        {
            ErNumber = erNumber,
            SubmissionNumber = 1,
            ValidationDate = validationDate,
            SubmissionValidUntil = validationDate.AddMonths(SubmissionValidityMonths),
            RequesterId = user.Id,
            Status = ErStatus.Draft
        };

        ApplyFields(er, request);
        _db.EngineeringRequests.Add(er);
        await _db.SaveChangesAsync();

        if (request.Submit)
            await SubmitInternalAsync(er);

        return (await GetByIdAsync(er.Id, user))!;
    }

    public async Task<ErDetailDto?> UpdateAsync(int id, UpdateErRequest request, User user)
    {
        var er = await LoadErAsync(id);
        if (er is null) return null;

        if (er.RequesterId != user.Id && user.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only the requester can edit this ER.");

        if (er.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.ResubmitRequested))
            throw new InvalidOperationException("This ER cannot be edited in its current status.");

        ValidateRequest(request);

        // ER number never changes on edit
        ApplyFields(er, request);
        er.UpdatedAt = DateTime.UtcNow;

        // Resubmit path: bump submission number (max 2), reset verifications
        if (request.Submit && er.Status is ErStatus.Rejected or ErStatus.ResubmitRequested)
        {
            if (er.SubmissionNumber >= MaxSubmissions)
                throw new InvalidOperationException($"Maximum submissions ({MaxSubmissions}) reached for this ER.");

            if (er.ResubmitDeadline.HasValue && DateTime.UtcNow > er.ResubmitDeadline.Value)
                throw new InvalidOperationException($"Resubmit window expired ({ResubmitWindowDays} days).");

            er.SubmissionNumber += 1;
            ResetVerification(er);
        }

        await _db.SaveChangesAsync();

        if (request.Submit)
            await SubmitInternalAsync(er);

        return await GetByIdAsync(er.Id, user);
    }

    public async Task<ErDetailDto?> GetByIdAsync(int id, User user)
    {
        var er = await LoadErAsync(id);
        if (er is null) return null;
        return MapDetail(er, user);
    }

    public async Task<IReadOnlyList<ErListItemDto>> ListForUserAsync(User user)
    {
        var query = _db.EngineeringRequests
            .AsNoTracking()
            .Include(e => e.Requester)
            .AsQueryable();

        if (user.Role == UserRole.Requester)
            query = query.Where(e => e.RequesterId == user.Id);
        else if (user.Role is UserRole.Safety or UserRole.DeptHead or UserRole.QA)
            query = query.Where(e => e.RequesterId == user.Id
                || e.VerifiedBySafetyUserId == user.Id
                || e.CheckedByDeptHeadUserId == user.Id
                || e.CheckedByQaUserId == user.Id);
        // Admin / COO see all

        var items = await query.OrderByDescending(e => e.UpdatedAt).ToListAsync();
        return items.Select(MapListItem).ToList();
    }

    public async Task<IReadOnlyList<ErListItemDto>> ListVerificationQueueAsync(User user)
    {
        var query = _db.EngineeringRequests
            .AsNoTracking()
            .Include(e => e.Requester)
            .Where(e => e.Status == ErStatus.InProgress);

        query = user.Role switch
        {
            UserRole.Safety => query.Where(e =>
                e.VerifiedBySafetyUserId == user.Id && e.SafetyDecision == VerificationDecision.Pending),
            UserRole.DeptHead => query.Where(e =>
                e.CheckedByDeptHeadUserId == user.Id && e.DeptHeadDecision == VerificationDecision.Pending),
            UserRole.QA => query.Where(e =>
                e.CheckedByQaUserId == user.Id && e.QaDecision == VerificationDecision.Pending),
            UserRole.Admin => query,
            _ => query.Where(_ => false)
        };

        var items = await query.OrderBy(e => e.SubmittedAt).ToListAsync();
        return items.Select(MapListItem).ToList();
    }

    public async Task<IReadOnlyList<ErListItemDto>> ListCooQueueAsync(User user)
    {
        if (user.Role is not (UserRole.COO or UserRole.Admin))
            return Array.Empty<ErListItemDto>();

        var items = await _db.EngineeringRequests
            .AsNoTracking()
            .Include(e => e.Requester)
            .Where(e => e.Status == ErStatus.InProgress
                && e.SafetyDecision == VerificationDecision.Approved
                && e.DeptHeadDecision == VerificationDecision.Approved
                && e.QaDecision == VerificationDecision.Approved
                && e.CooDecision == VerificationDecision.Pending)
            .OrderBy(e => e.SubmittedAt)
            .ToListAsync();

        return items.Select(MapListItem).ToList();
    }

    public async Task<ErDetailDto?> VerifyAsync(int id, VerificationActionRequest request, User user)
    {
        if (request.Decision is not (VerificationDecision.Approved or VerificationDecision.Rejected))
            throw new InvalidOperationException("Verifiers may only Approve or Reject (Cancel). No field modifications are allowed.");

        var er = await LoadErAsync(id);
        if (er is null) return null;

        if (er.Status != ErStatus.InProgress)
            throw new InvalidOperationException("ER is not in verification.");

        var level = user.Role switch
        {
            UserRole.Safety when er.VerifiedBySafetyUserId == user.Id => VerificationLevel.Safety,
            UserRole.DeptHead when er.CheckedByDeptHeadUserId == user.Id => VerificationLevel.DeptHead,
            UserRole.QA when er.CheckedByQaUserId == user.Id => VerificationLevel.QA,
            UserRole.Admin => GuessAdminLevel(er),
            _ => throw new UnauthorizedAccessException("You are not assigned to verify this ER.")
        };

        ApplyVerifierDecision(er, level, request.Decision, request.Comment);
        await AddCommentAsync(er, user, request.Decision.ToString(), request.Comment ?? string.Empty);
        er.UpdatedAt = DateTime.UtcNow;

        if (request.Decision == VerificationDecision.Rejected)
        {
            er.Status = ErStatus.Rejected;
            er.Outcome = ErOutcome.Fail;
            er.ResubmitDeadline = DateTime.UtcNow.AddDays(ResubmitWindowDays);
            await NotifyRequesterAsync(er, $"ER {er.DisplayErNumber} rejected",
                $"Your Engineering Request was rejected at {level} level. Comment: {request.Comment}");
        }
        else if (AllVerifiersApproved(er))
        {
            await NotifyCooAsync(er);
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(er.Id, user);
    }

    public async Task<ErDetailDto?> CooActAsync(int id, CooActionRequest request, User user)
    {
        if (user.Role is not (UserRole.COO or UserRole.Admin))
            throw new UnauthorizedAccessException("Only COO can perform this action.");

        if (request.Decision is not (VerificationDecision.Approved or VerificationDecision.Rejected or VerificationDecision.Resubmit))
            throw new InvalidOperationException("COO may Approve, Reject, or Re-Submit.");

        var er = await LoadErAsync(id);
        if (er is null) return null;

        if (er.Status != ErStatus.InProgress || !AllVerifiersApproved(er))
            throw new InvalidOperationException("ER is not ready for COO approval.");

        er.CooDecision = request.Decision;
        er.CooComment = request.Comment;
        er.CooDecidedAt = DateTime.UtcNow;
        er.ApprovedByCooUserId = user.Id;
        er.UpdatedAt = DateTime.UtcNow;

        switch (request.Decision)
        {
            case VerificationDecision.Approved:
                er.Status = ErStatus.Approved;
                er.Outcome = request.Outcome is ErOutcome.Pass or ErOutcome.Fail or ErOutcome.Resubmit
                    ? request.Outcome.Value
                    : ErOutcome.Pass;
                break;
            case VerificationDecision.Rejected:
                er.Status = ErStatus.Rejected;
                er.Outcome = ErOutcome.Fail;
                er.ResubmitDeadline = DateTime.UtcNow.AddDays(ResubmitWindowDays);
                break;
            case VerificationDecision.Resubmit:
                er.Status = ErStatus.ResubmitRequested;
                er.Outcome = ErOutcome.Resubmit;
                er.ResubmitDeadline = DateTime.UtcNow.AddDays(ResubmitWindowDays);
                break;
        }

        await AddCommentAsync(er, user, $"COO-{request.Decision}", request.Comment ?? string.Empty);
        await NotifyRequesterAsync(er, $"ER {er.DisplayErNumber} — COO {request.Decision}",
            $"COO decision: {request.Decision}. Comment: {request.Comment}");

        await _db.SaveChangesAsync();
        return await GetByIdAsync(er.Id, user);
    }

    public async Task<AttachmentDto?> AddAttachmentAsync(int erId, IFormFile file, User user)
    {
        var er = await LoadErAsync(erId);
        if (er is null) return null;

        if (er.RequesterId != user.Id && user.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only the requester can upload attachments.");

        if (er.Status is ErStatus.Approved or ErStatus.Closed)
            throw new InvalidOperationException("Cannot attach files to a closed/approved ER.");

        var saved = await _files.SaveAsync(file);
        var attachment = new ErAttachment
        {
            EngineeringRequestId = er.Id,
            FileName = file.FileName,
            StoredFileName = saved.StoredFileName,
            ContentType = saved.ContentType,
            FileSizeBytes = saved.Size,
            IsImage = saved.IsImage,
            UploadedByUserId = user.Id
        };

        _db.ErAttachments.Add(attachment);
        er.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AttachmentDto(
            attachment.Id,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.IsImage,
            _files.GetRelativeUrl(attachment.StoredFileName),
            attachment.UploadedAt);
    }

    public async Task<bool> DeleteAttachmentAsync(int erId, int attachmentId, User user)
    {
        var er = await LoadErAsync(erId);
        if (er is null) return false;

        if (er.RequesterId != user.Id && user.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only the requester can delete attachments.");

        if (er.Status is not (ErStatus.Draft or ErStatus.Rejected or ErStatus.ResubmitRequested))
            throw new InvalidOperationException("Attachments cannot be removed after submission.");

        var attachment = er.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null) return false;

        _files.Delete(attachment.StoredFileName);
        _db.ErAttachments.Remove(attachment);
        await _db.SaveChangesAsync();
        return true;
    }

    // --- helpers ---

    private async Task SubmitInternalAsync(EngineeringRequest er)
    {
        if (er.VerifiedBySafetyUserId is null || er.CheckedByDeptHeadUserId is null || er.CheckedByQaUserId is null)
            throw new InvalidOperationException("Select Verified By (Safety), Checked By (Dept Head), and Checked By (QA) before submit.");

        if (!er.ReasonCostDown && !er.ReasonAlternativeSourcing && !er.ReasonOthers)
            throw new InvalidOperationException("Select at least one Reason for Change.");

        er.Status = ErStatus.InProgress;
        er.SubmittedAt = DateTime.UtcNow;
        er.UpdatedAt = DateTime.UtcNow;
        er.Outcome = ErOutcome.None;
        ResetVerification(er, keepAssignees: true);

        await _db.SaveChangesAsync();

        var safety = await _db.Users.FindAsync(er.VerifiedBySafetyUserId);
        var dept = await _db.Users.FindAsync(er.CheckedByDeptHeadUserId);
        var qa = await _db.Users.FindAsync(er.CheckedByQaUserId);

        foreach (var u in new[] { safety, dept, qa }.Where(x => x is not null))
        {
            await _email.NotifyAsync(er, u!.Email,
                $"ER {er.DisplayErNumber} awaiting your verification",
                $"Engineering Request '{er.Title}' has been submitted and requires your review.");
        }
    }

    private static void ResetVerification(EngineeringRequest er, bool keepAssignees = true)
    {
        er.SafetyDecision = VerificationDecision.Pending;
        er.DeptHeadDecision = VerificationDecision.Pending;
        er.QaDecision = VerificationDecision.Pending;
        er.CooDecision = VerificationDecision.Pending;
        er.SafetyComment = null;
        er.DeptHeadComment = null;
        er.QaComment = null;
        er.CooComment = null;
        er.SafetyDecidedAt = null;
        er.DeptHeadDecidedAt = null;
        er.QaDecidedAt = null;
        er.CooDecidedAt = null;
        er.ApprovedByCooUserId = null;
        if (!keepAssignees)
        {
            // assignees preserved by default
        }
    }

    private static void ApplyVerifierDecision(
        EngineeringRequest er,
        VerificationLevel level,
        VerificationDecision decision,
        string? comment)
    {
        switch (level)
        {
            case VerificationLevel.Safety:
                if (er.SafetyDecision != VerificationDecision.Pending)
                    throw new InvalidOperationException("Safety has already decided.");
                er.SafetyDecision = decision;
                er.SafetyComment = comment;
                er.SafetyDecidedAt = DateTime.UtcNow;
                break;
            case VerificationLevel.DeptHead:
                if (er.DeptHeadDecision != VerificationDecision.Pending)
                    throw new InvalidOperationException("Dept Head has already decided.");
                er.DeptHeadDecision = decision;
                er.DeptHeadComment = comment;
                er.DeptHeadDecidedAt = DateTime.UtcNow;
                break;
            case VerificationLevel.QA:
                if (er.QaDecision != VerificationDecision.Pending)
                    throw new InvalidOperationException("QA has already decided.");
                er.QaDecision = decision;
                er.QaComment = comment;
                er.QaDecidedAt = DateTime.UtcNow;
                break;
        }
    }

    private static VerificationLevel GuessAdminLevel(EngineeringRequest er)
    {
        if (er.SafetyDecision == VerificationDecision.Pending) return VerificationLevel.Safety;
        if (er.DeptHeadDecision == VerificationDecision.Pending) return VerificationLevel.DeptHead;
        if (er.QaDecision == VerificationDecision.Pending) return VerificationLevel.QA;
        throw new InvalidOperationException("No pending verification level.");
    }

    private static bool AllVerifiersApproved(EngineeringRequest er) =>
        er.SafetyDecision == VerificationDecision.Approved
        && er.DeptHeadDecision == VerificationDecision.Approved
        && er.QaDecision == VerificationDecision.Approved;

    private static void ValidateRequest(CreateErRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException("Title is required.");
        if (request.SampleQuantity is < 0)
            throw new InvalidOperationException("Sample quantity must be a non-negative number.");
        if (request.ApplicableToChemicalOrMaterials)
        {
            // fields optional but visible — no hard require for initial version
        }
    }

    private static void ApplyFields(EngineeringRequest er, CreateErRequest request)
    {
        er.Division = request.Division;
        er.Title = request.Title.Trim();
        er.Department = request.Department;
        er.Product = request.Product;
        er.Customer = request.Customer ?? string.Empty;
        er.Process = request.Process ?? string.Empty;
        er.ReasonCostDown = request.ReasonCostDown;
        er.ReasonAlternativeSourcing = request.ReasonAlternativeSourcing;
        er.ReasonOthers = request.ReasonOthers;
        er.ReasonOthersText = request.ReasonOthersText;
        er.PresentDetails = request.PresentDetails;
        er.NewDetails = request.NewDetails;
        er.Merit = request.Merit;
        er.Demerit = request.Demerit;
        er.MaterialDisposition = request.MaterialDisposition;
        er.SampleQuantity = request.SampleQuantity;
        er.TestLotType = request.TestLotType;
        er.TestLotDescription = request.TestLotDescription;
        er.TestLotCodeSerial = request.TestLotCodeSerial;
        er.ApplicableToChemicalOrMaterials = request.ApplicableToChemicalOrMaterials;
        er.SafetyDataSheet = request.ApplicableToChemicalOrMaterials ? request.SafetyDataSheet : null;
        er.ChemicalLabel = request.ApplicableToChemicalOrMaterials ? request.ChemicalLabel : null;
        er.ChemicalClassification = request.ApplicableToChemicalOrMaterials ? request.ChemicalClassification : null;
        er.ChemicalInventorySystem = request.ApplicableToChemicalOrMaterials ? request.ChemicalInventorySystem : null;
        er.VerifiedBySafetyUserId = request.VerifiedBySafetyUserId;
        er.CheckedByDeptHeadUserId = request.CheckedByDeptHeadUserId;
        er.CheckedByQaUserId = request.CheckedByQaUserId;
    }

    private async Task AddCommentAsync(EngineeringRequest er, User user, string action, string comment)
    {
        _db.ErComments.Add(new ErComment
        {
            EngineeringRequestId = er.Id,
            UserId = user.Id,
            Action = action,
            Comment = comment
        });
        await Task.CompletedTask;
    }

    private async Task NotifyRequesterAsync(EngineeringRequest er, string subject, string body)
    {
        var requester = er.Requester ?? await _db.Users.FindAsync(er.RequesterId);
        if (requester is null) return;
        await _email.NotifyAsync(er, requester.Email, subject, body);
    }

    private async Task NotifyCooAsync(EngineeringRequest er)
    {
        var coos = await _db.Users.Where(u => u.Role == UserRole.COO && u.IsActive).ToListAsync();
        foreach (var coo in coos)
        {
            await _email.NotifyAsync(er, coo.Email,
                $"ER {er.DisplayErNumber} ready for COO approval",
                $"All three verifiers approved '{er.Title}'. Awaiting COO decision.");
        }
    }

    private async Task<EngineeringRequest?> LoadErAsync(int id) =>
        await _db.EngineeringRequests
            .Include(e => e.Requester)
            .Include(e => e.Attachments)
            .Include(e => e.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(e => e.Id == id);

    private ErListItemDto MapListItem(EngineeringRequest er)
    {
        var days = Math.Max(0, (int)Math.Ceiling((er.SubmissionValidUntil - DateTime.UtcNow).TotalDays));
        return new ErListItemDto(
            er.Id,
            er.ErNumber,
            er.SubmissionNumber,
            er.DisplayErNumber,
            er.Title,
            er.Division.ToString(),
            er.Department,
            er.Product,
            er.Status.ToString(),
            er.Outcome.ToString(),
            er.Requester?.FullName ?? "",
            er.CreatedAt,
            er.SubmittedAt,
            er.ValidationDate,
            er.SubmissionValidUntil,
            days);
    }

    private ErDetailDto MapDetail(EngineeringRequest er, User current)
    {
        var safetyName = er.VerifiedBySafetyUserId.HasValue
            ? _db.Users.Where(u => u.Id == er.VerifiedBySafetyUserId).Select(u => u.FullName).FirstOrDefault()
            : null;
        var deptName = er.CheckedByDeptHeadUserId.HasValue
            ? _db.Users.Where(u => u.Id == er.CheckedByDeptHeadUserId).Select(u => u.FullName).FirstOrDefault()
            : null;
        var qaName = er.CheckedByQaUserId.HasValue
            ? _db.Users.Where(u => u.Id == er.CheckedByQaUserId).Select(u => u.FullName).FirstOrDefault()
            : null;

        var days = Math.Max(0, (int)Math.Ceiling((er.SubmissionValidUntil - DateTime.UtcNow).TotalDays));
        var canEdit = (er.RequesterId == current.Id || current.Role == UserRole.Admin)
            && er.Status is ErStatus.Draft or ErStatus.Rejected or ErStatus.ResubmitRequested;
        var canVerify = er.Status == ErStatus.InProgress && (
            (current.Role == UserRole.Safety && er.VerifiedBySafetyUserId == current.Id && er.SafetyDecision == VerificationDecision.Pending)
            || (current.Role == UserRole.DeptHead && er.CheckedByDeptHeadUserId == current.Id && er.DeptHeadDecision == VerificationDecision.Pending)
            || (current.Role == UserRole.QA && er.CheckedByQaUserId == current.Id && er.QaDecision == VerificationDecision.Pending)
            || current.Role == UserRole.Admin);
        var canCoo = (current.Role is UserRole.COO or UserRole.Admin)
            && er.Status == ErStatus.InProgress
            && AllVerifiersApproved(er)
            && er.CooDecision == VerificationDecision.Pending;

        return new ErDetailDto(
            er.Id,
            er.ErNumber,
            er.SubmissionNumber,
            er.DisplayErNumber,
            er.Division.ToString(),
            er.ValidationDate,
            er.SubmissionValidUntil,
            er.ResubmitDeadline,
            days,
            er.Title,
            er.Department,
            er.Product,
            er.Customer,
            er.Process,
            er.ReasonCostDown,
            er.ReasonAlternativeSourcing,
            er.ReasonOthers,
            er.ReasonOthersText,
            er.PresentDetails,
            er.NewDetails,
            er.Merit,
            er.Demerit,
            er.MaterialDisposition,
            er.SampleQuantity,
            er.TestLotType?.ToString(),
            er.TestLotDescription,
            er.TestLotCodeSerial,
            er.ApplicableToChemicalOrMaterials,
            er.SafetyDataSheet,
            er.ChemicalLabel,
            er.ChemicalClassification,
            er.ChemicalInventorySystem,
            er.VerifiedBySafetyUserId,
            er.CheckedByDeptHeadUserId,
            er.CheckedByQaUserId,
            safetyName,
            deptName,
            qaName,
            er.SafetyDecision.ToString(),
            er.DeptHeadDecision.ToString(),
            er.QaDecision.ToString(),
            er.CooDecision.ToString(),
            er.SafetyComment,
            er.DeptHeadComment,
            er.QaComment,
            er.CooComment,
            er.Status.ToString(),
            er.Outcome.ToString(),
            er.RequesterId,
            er.Requester?.FullName ?? "",
            er.CreatedAt,
            er.UpdatedAt,
            er.SubmittedAt,
            er.Attachments.OrderBy(a => a.UploadedAt).Select(a => new AttachmentDto(
                a.Id, a.FileName, a.ContentType, a.FileSizeBytes, a.IsImage,
                _files.GetRelativeUrl(a.StoredFileName), a.UploadedAt)).ToList(),
            er.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentDto(
                c.Id, c.User?.FullName ?? "", c.Action, c.Comment, c.CreatedAt)).ToList(),
            canEdit,
            canVerify,
            canCoo);
    }
}
