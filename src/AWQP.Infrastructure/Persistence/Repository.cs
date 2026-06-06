using System.Linq.Expressions;
using AWQP.Application.Interfaces;
using AWQP.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Persistence;

public sealed class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ManufacturingDbContext _dbContext;

    public EfRepository(ManufacturingDbContext dbContext) => _dbContext = dbContext;

    public IQueryable<TEntity> Query() => _dbContext.Set<TEntity>().AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = predicate is null ? _dbContext.Set<TEntity>() : _dbContext.Set<TEntity>().Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    public void Update(TEntity entity) => _dbContext.Set<TEntity>().Update(entity);
    public void Remove(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);
}
