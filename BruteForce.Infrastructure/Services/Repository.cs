
using System.Linq.Expressions;
using BruteForce.Domain.Models;
using BruteForce.Domain.Entities;
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using BruteForce.Infrastructure.Exceptions;

namespace BruteForce.Infrastructure.Services;

public class Repository<TEntity>
    (IApplicationDbContext appDbContext, ICurrentUser currentUser, ICurrentTenant currentTenant)
    : Repository<TEntity, long>(appDbContext, currentUser, currentTenant), IRepository<TEntity> where TEntity : AggregateRoot<long>
{ }

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    #region Fields
    private readonly DateTime _now = DateTime.Now;
    private readonly string _entityTypeName = nameof(TEntity);
    private readonly string _actor;
    private readonly int _tenantId;
    private readonly bool _auditCreation = typeof(IAuditCreation).IsAssignableFrom(typeof(TEntity));
    private readonly bool _auditUpdate = typeof(IAuditUpdate).IsAssignableFrom(typeof(TEntity));
    private readonly bool _softDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
    private readonly bool _hasTenant = typeof(IHasTenant).IsAssignableFrom(typeof(TEntity));
    private readonly IApplicationDbContext _appDbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    #endregion Fields

    public Repository(IApplicationDbContext appDbContext, ICurrentUser currentUser, ICurrentTenant currentTenant)
    {
        _appDbContext = appDbContext;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _actor = $"{_currentUser.GetFullName()} ({_currentUser.GetUserName()})";
        _tenantId = _currentTenant.GetTenantId()!.Value;
        _dbContext = (_appDbContext as DbContext)!;
        _dbSet = _dbContext.Set<TEntity>();
    }

    #region Read
    public IQueryable<TEntity> AsQueryable()
        => _dbSet.Where(e => !_hasTenant || (e as IHasTenant)!.TenantId == _tenantId)
                 .Where(e => !_softDelete || !(e as ISoftDelete)!.IsDeleted);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().CountAsync(cancellationToken);

    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().LongCountAsync(cancellationToken);

    public async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<List<TEntity>> FindAllAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity?> FindByIdAsync(TKey Id, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(e => e.Id.Equals(Id)).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> FindByIdAndTrackAsync(int Id, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(e => e.Id.Equals(Id)).FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> FindAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await AsQueryable().Where(predicate).FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await PagedResult<TEntity>.CreateAsync(AsQueryable(), pageSize, pageNumber, cancellationToken);

    public async Task<PagedResult<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        => await PagedResult<TEntity>.CreateAsync(AsQueryable().Where(predicate), pageSize, pageNumber, cancellationToken);

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().ToListAsync(cancellationToken);

    public async Task<List<TEntity>> GetAllAndTrackAsync(CancellationToken cancellationToken = default)
        => await AsQueryable().AsNoTracking().ToListAsync(cancellationToken);
    #endregion Read

    #region Add
    public async Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_auditCreation)
            entity = ((entity as IAuditCreation)!.SetCreatedBy(_actor).SetCreatedDate(_now) as TEntity)!;

        if (_hasTenant)
            entity = ((entity as IHasTenant)!.SetTenantId(_tenantId) as TEntity)!;

        await _dbSet.AddAsync(entity, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (!entities.Any())
            return 0;

        if (_auditCreation)
            entities = Enumerable.Cast<IAuditCreation>(entities)
                      .Select(e =>
                          e.SetCreatedBy(_actor)
                          .SetCreatedDate(_now)
                      )
                      .Cast<TEntity>();

        if (_hasTenant)
            entities = Enumerable.Cast<IHasTenant>(entities)
                      .Select(e => e.SetTenantId(_tenantId))
                      .Cast<TEntity>();

        await _dbSet.AddRangeAsync(entities, cancellationToken);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }
    #endregion Add

    #region Remove
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

        var entity = await AsQueryable().FirstOrDefaultAsync(e => e.Id.Equals(Id), cancellationToken) ??
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_softDelete)
        {
            entity = (entity as ISoftDelete)!.SetIsDeleted(true).SetDeletedBy(_actor).SetDeletedDate(_now) as TEntity;
            _dbSet.Update(entity!);
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

        if (_hasTenant)
            foreach (TEntity entity in entities)
            {
                if ((entity as IHasTenant)!.TenantId != _tenantId)
                    throw new RepositoryException("You are not the owner tenant");
            }

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
    #endregion Remove

    #region Update
    public async Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new RepositoryException($"Provided {_entityTypeName} was null");

        if (_hasTenant && (entity as IHasTenant)!.TenantId != _tenantId)
            throw new RepositoryException("You are not the owner tenant");

        if (_auditUpdate)
            entity = ((entity as IAuditUpdate)!
                .SetUpdatedBy(_actor)
                .SetUpdatedDate(_now)
                as TEntity)!;

        _dbSet.Update(entity);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new RepositoryException($"Provided enumerable of {_entityTypeName}s was null");

        if (_hasTenant)
            foreach (TEntity entity in entities)
            {
                if ((entity as IHasTenant)!.TenantId != _tenantId)
                    throw new RepositoryException("You are not the owner tenant");
            }

        if (!entities.Any())
            return 0;

        if (_auditUpdate)
            entities = Enumerable.Cast<IAuditUpdate>(entities)
                      .Select(e =>
                          e.SetUpdatedBy(_actor)
                           .SetUpdatedDate(_now)
                      ).Cast<TEntity>();

        _dbSet.UpdateRange(entities);

        return commitImmediately ? await _dbContext.SaveChangesAsync(cancellationToken) : 0;
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default)
    {
        if (!_auditUpdate)
            return await AsQueryable().Where(predicate).ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

        var taskAuditUpdate = AsQueryable()
                             .Where(predicate)
                             .ExecuteUpdateAsync(setters =>
                                 setters.SetProperty(e => (e as IAuditUpdate).UpdatedBy, _actor)
                                        .SetProperty(e => (e as IAuditUpdate).UpdatedDate, _now),
                                 cancellationToken
                             );

        var taskUserDefinedUpdate = AsQueryable()
                                   .Where(predicate)
                                   .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

        return (await Task.WhenAll(taskAuditUpdate, taskUserDefinedUpdate)).Sum();
    }
    #endregion Update

    #region Save
    public int SaveChanges() => _dbContext.SaveChanges();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);
    #endregion Save

    #region StateChange
    public void MarkAsAdded(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Added;

    public void MarkAsDeleted(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Deleted;

    public void MarkAsDetached(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Detached;

    public void MarkAsModified(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Modified;

    public void MarkAsUnchanged(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Unchanged;
    #endregion StateChange
}
