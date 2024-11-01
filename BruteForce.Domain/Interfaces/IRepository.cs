﻿
using System.Linq.Expressions;
using BruteForce.Domain.Models;
using BruteForce.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace BruteForce.Domain.Interfaces;

public interface IRepository<TEntity> : IRepository<TEntity, long> where TEntity : AggregateRoot<long> { }

public interface IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : IComparable
{
    IQueryable<TEntity> AsQueryable();

    #region Query
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(TKey Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAndTrackAsync(int Id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAndTrackAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAndTrackAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> GetPagedAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> GetFilteredPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    #endregion Query

    #region Add
    Task<int> AddAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);
    #endregion Add

    #region Remove
    Task<int> RemoveByIdAsync(TKey Id, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);
    #endregion Remove

    #region Update
    Task<int> UpdateAsync(TEntity entity, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, bool commitImmediately = false, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellation = default);
    #endregion Update

    #region StateChange
    void MarkAsAdded(TEntity entity);
    void MarkAsDeleted(TEntity entity);
    void MarkAsDetached(TEntity entity);
    void MarkAsModified(TEntity entity);
    void MarkAsUnchanged(TEntity entity);
    #endregion StateChange

    #region Save
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    #endregion Save
}
