
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BruteForce.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    public UnitOfWork (IApplicationDbContext applicationDbContext) => _dbContext = applicationDbContext as DbContext;

    public async Task<IDbContextTransaction> BeginTransactionAsync (IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        => await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

    public async Task CommitTransactionAsync (CancellationToken cancellationToken = default)
        => await _dbContext.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollBackTransactionAsync (CancellationToken cancellationToken = default) 
        => await _dbContext.Database.RollbackTransactionAsync(cancellationToken);

    public async Task<int> ExecuteQueryAsync (FormattableString formattableString, CancellationToken cancellationToken = default)
        => await _dbContext.Database.ExecuteSqlInterpolatedAsync(formattableString, cancellationToken);
}
