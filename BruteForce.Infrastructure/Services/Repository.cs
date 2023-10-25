
using System.Linq.Expressions;
using BruteForce.Domain.Entities;
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using BruteForce.Infrastructure.Exceptions;

namespace BruteForce.Infrastructure.Services;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot<int>
{
    private readonly DateTime _now = DateTime.Now;
    private readonly string _entityTypeName = nameof(TEntity);
    private readonly string _actor;
    private readonly bool _isAuditable = typeof(AuditableEntity<int>).IsAssignableFrom(typeof(TEntity));
    private readonly bool _softDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
    private readonly DbContext _dbContext;

    public Repository(IApplicationDbContext appDbContext, ICurrentUser currentUser)
    {
        _actor = currentUser.GetUserName();
        _dbContext = appDbContext as DbContext;
    }

    public DbSet<TEntity> DbSet() => _dbContext.Set<TEntity>();

    public IQueryable<TEntity> AsQueryable() => DbSet().AsQueryable();

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().CountAsync(cancellationToken);

    public async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<TEntity?> FindByIdAsync(int Id, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(e => e.Id == Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().Skip(pageSize * pageNumber).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().Skip(pageSize * pageNumber).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().ToListAsync(cancellationToken);

    public async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_isAuditable)
            entity = (entity as AuditableEntity<int>).SetCreatedBy(_actor).SetCreatedDate(_now) as TEntity;

        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        if (_isAuditable)
            entities = Enumerable.Cast<AuditableEntity<int>>(entities)
                      .Select(e =>
                          e.SetCreatedBy(_actor)
                          .SetCreatedDate(_now)
                      )
                      .Cast<TEntity>();

        await _dbContext.AddRangeAsync(entities, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveByIdAsync(int Id, CancellationToken cancellationToken = default)
        => await (_softDelete ?
                AsQueryable()
                    .Where(e => e.Id == Id)
                    .Cast<ISoftDelete>()
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(e => e.IsDeleted, IsDeleted => true)
                        .SetProperty(e => e.DeletedDate, DeletedDate => _now)
                        .SetProperty(e => e.DeletedBy, DeletedBy => _actor)
                        , cancellationToken
                    )
                :
                AsQueryable()
                    .Where(e => e.Id == Id)
                    .ExecuteDeleteAsync(cancellationToken)
        );

    public async Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        => entity is null ?
            throw new RepositoryException($"Provided {_entityTypeName} was null")
            :
            await RemoveByIdAsync(entity.Id, cancellationToken);

    public async Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        return await (_softDelete ?
                AsQueryable()
                    .Where(i => entities.Select(e => e.Id).Contains(i.Id))
                    .Cast<ISoftDelete>()
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(e => e.IsDeleted, IsDeleted => true)
                        .SetProperty(e => e.DeletedBy, DeletedBy => _actor)
                        .SetProperty(e => e.DeletedDate, DeletedDate => _now)
                    , cancellationToken
                    )
                :
                AsQueryable()
                    .Where(i => entities.Select(e => e.Id).Contains(i.Id))
                    .ExecuteDeleteAsync(cancellationToken)
        );
    }

    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        _dbContext.Set<TEntity>().Update(entity);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        _dbContext.Set<TEntity>().UpdateRange(entities);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

    public void MarkAsAdded(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Added;

    public void MarkAsDeleted(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Deleted;

    public void MarkAsDetached(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Detached;

    public void MarkAsModified(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Modified;

    public void MarkAsUnchanged(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Unchanged;
}

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    private readonly DateTime _now = DateTime.Now;
    private readonly string _entityTypeName = nameof(TEntity);
    private readonly string _actor;
    private readonly bool _isAuditable = typeof(AuditableEntity<TKey>).IsAssignableFrom(typeof(TEntity));
    private readonly bool _softDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
    private readonly DbContext _dbContext;

    public Repository(IApplicationDbContext appDbContext, ICurrentUser currentUser)
    {
        _actor = currentUser.GetUserName();
        _dbContext = appDbContext as DbContext;
    }

    public DbSet<TEntity> DbSet() => _dbContext.Set<TEntity>();

    public IQueryable<TEntity> AsQueryable() => DbSet().AsQueryable();

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().CountAsync(cancellationToken);

    public async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<TEntity?> FindByIdAsync(TKey Id, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(e => e.Id.Equals(Id)).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    
    public async Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().Skip(pageSize * pageNumber).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().Skip(pageSize * pageNumber).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().ToListAsync(cancellationToken);

    public async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_isAuditable)
            entity = (entity as AuditableEntity<TKey>).SetCreatedBy(_actor).SetCreatedDate(_now) as TEntity;

        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        if (_isAuditable)
            entities = Enumerable.Cast<AuditableEntity<TKey>>(entities)
                      .Select(e =>
                          e.SetCreatedBy(_actor)
                          .SetCreatedDate(_now)
                      )
                      .Cast<TEntity>();

        await _dbContext.AddRangeAsync(entities, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveByIdAsync(TKey Id, CancellationToken cancellationToken = default)
        => await (_softDelete ?
                AsQueryable()
                    .Where(e => e.Id.Equals(Id))
                    .Cast<ISoftDelete>()
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(e => e.IsDeleted, IsDeleted => true)
                        .SetProperty(e => e.DeletedDate, DeletedDate => _now)
                        .SetProperty(e => e.DeletedBy, DeletedBy => _actor)
                        , cancellationToken
                    )
                :
                AsQueryable()
                    .Where(e => e.Id.Equals(Id))
                    .ExecuteDeleteAsync(cancellationToken)
        );

    public async Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        => entity is null ?
            throw new RepositoryException($"Provided {_entityTypeName} was null")
            :
            await RemoveByIdAsync(entity.Id, cancellationToken);

    public async Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        return await (_softDelete ?
                AsQueryable()
                    .Where(i => entities.Select(e => e.Id).Contains(i.Id))
                    .Cast<ISoftDelete>()
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(e => e.IsDeleted, IsDeleted => true)
                        .SetProperty(e => e.DeletedBy, DeletedBy => _actor)
                        .SetProperty(e => e.DeletedDate, DeletedDate => _now)
                    , cancellationToken
                    )
                :
                AsQueryable()
                    .Where(i => entities.Select(e => e.Id).Contains(i.Id))
                    .ExecuteDeleteAsync(cancellationToken)
        );
    }

    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        _dbContext.Set<TEntity>().Update(entity);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        _dbContext.Set<TEntity>().UpdateRange(entities);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

    public void MarkAsAdded(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Added;

    public void MarkAsDeleted(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Deleted;

    public void MarkAsDetached(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Detached;

    public void MarkAsModified(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Modified;

    public void MarkAsUnchanged(TEntity entity) => _dbContext.Entry(entity).State = EntityState.Unchanged;
}
