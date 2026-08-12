using AWQP.Application.Interfaces;
using AWQP.Domain.Common;

namespace AWQP.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ManufacturingDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ManufacturingDbContext dbContext) => _dbContext = dbContext;

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new EfRepository<TEntity>(_dbContext);
            _repositories[type] = repository;
        }
        return (IRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);
}
