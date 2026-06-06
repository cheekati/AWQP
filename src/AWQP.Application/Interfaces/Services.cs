using AWQP.Application.Common;
using AWQP.Application.DTOs;
using AWQP.Domain.Entities;

namespace AWQP.Application.Interfaces;

public interface ITokenService
{
    AuthResponse CreateToken(ApplicationUser user, string refreshToken);
    string CreateRefreshToken();
    string HashToken(string token);
}

public interface ICurrentUserService
{
    string UserName { get; }
    Guid? UserId { get; }
    string? IpAddress { get; }
}

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IManufacturingService
{
    Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default);
    Task StartOperationAsync(OperationStartRequest request, CancellationToken cancellationToken = default);
    Task CompleteOperationAsync(OperationCompleteRequest request, CancellationToken cancellationToken = default);
    Task<TraceabilityDto?> GetGenealogyAsync(string serialNumber, CancellationToken cancellationToken = default);
}

public interface IQualityService
{
    Task<InspectionRecordDto> CreateInspectionAsync(CreateInspectionRequest request, CancellationToken cancellationToken = default);
}

public interface ICleanRoomService
{
    Task<EnvironmentalReadingDto> RecordReadingAsync(CreateEnvironmentalReadingRequest request, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardDto> GetExecutiveDashboardAsync(CancellationToken cancellationToken = default);
}

public interface IBarcodeService
{
    byte[] GenerateCode128Png(string value);
}

public interface IQrCodeService
{
    byte[] GenerateQrPng(string value);
}
