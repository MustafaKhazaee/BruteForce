
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BruteForce.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollBackTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> ExecuteQueryAsync(FormattableString formattableString, CancellationToken cancellationToken = default);
}
