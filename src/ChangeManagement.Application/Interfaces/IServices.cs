using ChangeManagement.Application.DTOs.Auth;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.Dashboard;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.MasterData;
using ChangeManagement.Application.DTOs.Notifications;
using ChangeManagement.Application.DTOs.Users;
using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ChangeManagement.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
    bool IsAuthenticated { get; }
}

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IMasterDataService
{
    Task<ApiResponse<IReadOnlyList<LookupDto>>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<LookupDto>>> GetDivisionsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<LookupDto>>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<LookupDto>>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<LookupDto>> CreateDepartmentAsync(CreateLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LookupDto>> CreateDivisionAsync(CreateLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LookupDto>> CreateProductAsync(CreateLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LookupDto>> CreateCustomerAsync(CreateLookupRequest request, CancellationToken cancellationToken = default);
}

public interface IEngineeringRequestService
{
    Task<ApiResponse<EngineeringRequestDetailDto>> CreateAsync(CreateEngineeringRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EngineeringRequestDetailDto>> UpdateAsync(Guid id, UpdateEngineeringRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EngineeringRequestDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetMyRequestsAsync(EngineeringRequestQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetAllAsync(EngineeringRequestQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<EngineeringRequestDetailDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAttachmentService
{
    Task<ApiResponse<IReadOnlyList<AttachmentDto>>> UploadAsync(Guid erId, IFormFileCollection files, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid erId, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType, string FileName)?> DownloadAsync(Guid erId, Guid attachmentId, CancellationToken cancellationToken = default);
}

public interface IVerificationService
{
    Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetAssignedRequestsAsync(VerificationStage stage, EngineeringRequestQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<EngineeringRequestDetailDto>> ActAsync(Guid erId, VerificationStage stage, VerificationActionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommentDto>> AddCommentAsync(Guid erId, AddCommentRequest request, CancellationToken cancellationToken = default);
}

public interface ICooApprovalService
{
    Task<ApiResponse<PagedResult<EngineeringRequestListDto>>> GetPendingAsync(EngineeringRequestQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<EngineeringRequestDetailDto>> ActAsync(Guid erId, CooActionRequest request, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task NotifyAsync(Guid userId, NotificationType type, string subject, string message, Guid? erId = null, bool sendEmail = true, CancellationToken cancellationToken = default);
    Task NotifyRoleAsync(string roleName, NotificationType type, string subject, string message, Guid? erId = null, bool sendEmail = true, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetStatsAsync(CancellationToken cancellationToken = default);
}

public interface IFileStorageService
{
    Task<(string StoredFileName, string StoragePath, bool IsImage)> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType)?> OpenReadAsync(string storagePath, string contentType, CancellationToken cancellationToken = default);
}
