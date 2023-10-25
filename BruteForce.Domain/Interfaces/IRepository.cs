using BruteForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BruteForce.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : AggregateRoot<int>
{
    DbSet<TEntity> DbSet();
    IQueryable<TEntity> AsQueryable();

    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(int Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<int> RemoveByIdAsync(int Id, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Use UpdateAsync method with "SetPropertyCalls" parameter for better performance
    /// </summary>
    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Use UpdateAsync method with "SetPropertyCalls" parameter for better performance
    /// </summary>
    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellation = default);

    void MarkAsAdded(TEntity entity);
    void MarkAsDeleted(TEntity entity);
    void MarkAsDetached(TEntity entity);
    void MarkAsModified(TEntity entity);
    void MarkAsUnchanged(TEntity entity);
}

public interface IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    DbSet<TEntity> DbSet();
    IQueryable<TEntity> AsQueryable();

    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(TKey Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<int> RemoveByIdAsync(TKey Id, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Use UpdateAsync method with "SetPropertyCalls" parameter for better performance
    /// </summary>
    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Use UpdateAsync method with "SetPropertyCalls" parameter for better performance
    /// </summary>
    [Obsolete("Use UpdateAsync method with \"SetPropertyCalls\" parameter for better performance")]
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellation = default);

    void MarkAsAdded(TEntity entity);
    void MarkAsDeleted(TEntity entity);
    void MarkAsDetached(TEntity entity);
    void MarkAsModified(TEntity entity);
    void MarkAsUnchanged(TEntity entity);
}
