using System.Linq.Expressions;
using AWQP.Application.Common;
using AWQP.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Persistence;

public sealed class EfRepository<TEntity>(AwqpDbContext dbContext) : IRepository<TEntity> where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>().AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<TEntity>().Where(x => !x.IsDeleted);
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        entity.IsDeleted = true;
        dbContext.Set<TEntity>().Update(entity);
    }
}

public sealed class UnitOfWork(AwqpDbContext dbContext) : IUnitOfWork
{
    private readonly Dictionary<Type, object> repositories = [];

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        if (!repositories.TryGetValue(type, out var repository))
        {
            repository = new EfRepository<TEntity>(dbContext);
            repositories[type] = repository;
        }

        return (IRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
