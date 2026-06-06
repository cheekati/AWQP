using System.Linq.Expressions;
using AWQP.Domain.Common;

namespace AWQP.Application.Common;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    string IpAddress { get; }
}

public interface IJwtTokenService
{
    Task<AuthResponse> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}

public interface IBarcodeService
{
    string GenerateCode128Svg(string value, int height = 72);
    string GenerateQrCodeSvg(string value, int pixelsPerModule = 8);
}

public interface ITraceabilityService
{
    Task<GenealogyDto?> GetGenealogyAsync(string serialNumber, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<ProductionDashboardDto> GetProductionDashboardAsync(CancellationToken cancellationToken = default);
    Task<QualityDashboardDto> GetQualityDashboardAsync(CancellationToken cancellationToken = default);
    Task<InventoryDashboardDto> GetInventoryDashboardAsync(CancellationToken cancellationToken = default);
    Task<CleanRoomDashboardDto> GetCleanRoomDashboardAsync(CancellationToken cancellationToken = default);
    Task<ShippingDashboardDto> GetShippingDashboardAsync(CancellationToken cancellationToken = default);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
