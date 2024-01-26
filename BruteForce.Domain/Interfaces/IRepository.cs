
using System.Linq.Expressions;
using BruteForce.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace BruteForce.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : AggregateRoot<int>
{
    IQueryable<TEntity> AsQueryable();

    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(int Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAndTrackAsync(int Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAndTrackAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);

    Task<int> RemoveByIdAsync(int Id, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);

    Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// This method will commit immediately.
    /// You will have to audit it yourself.
    /// You need to take care of tenants too.
    /// </summary>
    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellation = default);
    
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void MarkAsAdded(TEntity entity);
    void MarkAsDeleted(TEntity entity);
    void MarkAsDetached(TEntity entity);
    void MarkAsModified(TEntity entity);
    void MarkAsUnchanged(TEntity entity);
}

public interface IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    IQueryable<TEntity> AsQueryable();

    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(TKey Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAndTrackAsync(int Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAndTrackAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);

    Task<int> RemoveByIdAsync(TKey Id, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);

    Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// This method will commit immediately.
    /// You will have to audit it yourself.
    /// You need to take care of tenants too.
    /// </summary>
    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellation = default);

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void MarkAsAdded(TEntity entity);
    void MarkAsDeleted(TEntity entity);
    void MarkAsDetached(TEntity entity);
    void MarkAsModified(TEntity entity);
    void MarkAsUnchanged(TEntity entity);
}
