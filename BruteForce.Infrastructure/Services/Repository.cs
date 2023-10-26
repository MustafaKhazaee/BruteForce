
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
    private readonly IApplicationDbContext _appDbContext;
    private readonly ICurrentUser _currentUser;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(IApplicationDbContext appDbContext, ICurrentUser currentUser)
    {
        _appDbContext = appDbContext;
        _currentUser = currentUser;
        _actor = _currentUser.GetUserName();
        _dbContext = _appDbContext as DbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public IRepository<T> ChangeEntity<T>() where T : AggregateRoot<int> =>
        new Repository<T>(_appDbContext, _currentUser);

    public IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();

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

    public async Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_isAuditable)
            entity = (entity as AuditableEntity<int>).SetCreatedBy(_actor).SetCreatedDate(_now) as TEntity;

        await _dbSet.AddAsync(entity, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
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

        await _dbSet.AddRangeAsync(entities, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> RemoveByIdAsync(int Id, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (commitImmediately)
            return await (_softDelete ?
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

        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == Id) ??
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_softDelete)
        {
            entity = (entity as ISoftDelete).SetIsDeleted(true).SetDeletedBy(_actor).SetDeletedDate(_now) as TEntity;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        return 0;
    }

    public async Task<int> RemoveAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
        => entity is null ?
            throw new RepositoryException($"Provided {_entityTypeName} was null")
            :
            await RemoveByIdAsync(entity.Id, commitImmediately, cancellationToken);

    public async Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        if (commitImmediately)
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

        if (_softDelete)
        {
            entities = Enumerable.Cast<ISoftDelete>(entities).Select(
                e => e.SetDeletedBy(_actor).SetDeletedDate(_now).SetIsDeleted(true)
            ).Cast<TEntity>();

            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }

        return 0;
    }

    public async Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        _dbSet.Update(entity);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        _dbSet.UpdateRange(entities);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

    public int SaveChanges() => _dbContext.SaveChanges();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);

    public void MarkAsAdded(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Added;

    public void MarkAsDeleted(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Deleted;

    public void MarkAsDetached(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Detached;

    public void MarkAsModified(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Modified;

    public void MarkAsUnchanged(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Unchanged;
}

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    private readonly DateTime _now = DateTime.Now;
    private readonly string _entityTypeName = nameof(TEntity);
    private readonly string _actor;
    private readonly bool _isAuditable = typeof(AuditableEntity<TKey>).IsAssignableFrom(typeof(TEntity));
    private readonly bool _softDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
    private readonly IApplicationDbContext _appDbContext;
    private readonly ICurrentUser _currentUser;
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(IApplicationDbContext appDbContext, ICurrentUser currentUser)
    {
        _appDbContext = appDbContext;
        _currentUser = currentUser;
        _actor = _currentUser.GetUserName();
        _dbContext = _appDbContext as DbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public IRepository<T, K> ChangeEntity<T, K>() where T : AggregateRoot<K> where K : IComparable => 
        new Repository<T, K>(_appDbContext, _currentUser);

    public IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();

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

    public async Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_isAuditable)
            entity = (entity as AuditableEntity<TKey>).SetCreatedBy(_actor).SetCreatedDate(_now) as TEntity;

        await _dbSet.AddAsync(entity, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
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

        await _dbSet.AddRangeAsync(entities, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> RemoveByIdAsync(TKey Id, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (commitImmediately)
            return await (_softDelete ?
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

        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id.Equals(Id)) ?? 
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_softDelete)
        {
            entity = (entity as ISoftDelete).SetIsDeleted(true).SetDeletedBy(_actor).SetDeletedDate(_now) as TEntity;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        return 0;
    }

    public async Task<int> RemoveAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
        => entity is null ?
            throw new RepositoryException($"Provided {_entityTypeName} was null")
            :
            await RemoveByIdAsync(entity.Id, commitImmediately, cancellationToken);

    public async Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        if (commitImmediately)
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

        if (_softDelete)
        {
            entities = Enumerable.Cast<ISoftDelete>(entities).Select(
                e => e.SetDeletedBy(_actor).SetDeletedDate(_now).SetIsDeleted(true)
            ).Cast<TEntity>();

            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }

        return 0;
    }

    public async Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        _dbSet.Update(entity);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        _dbSet.UpdateRange(entities);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

    public int SaveChanges() => _dbContext.SaveChanges();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);

    public void MarkAsAdded(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Added;

    public void MarkAsDeleted(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Deleted;

    public void MarkAsDetached(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Detached;

    public void MarkAsModified(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Modified;

    public void MarkAsUnchanged(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Unchanged;
}
