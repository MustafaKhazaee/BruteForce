
using BruteForce.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BruteForce.Domain.Models;

public class PagedResult<T>
{
    public long TotalRecords { set; get; }
    public long TotalPages { set; get; }
    public long CurrentPage { set; get; }
    public long PageSize { set; get; }
    public bool HasNextPage { set; get; }
    public bool HasPreviousPage { set; get; }
    public List<T> Data { set; get; }

    /// <summary>
    /// It is better is you use the CreateAsync method :)
    /// </summary>
    /// <param name="totalRecords"></param>
    /// <param name="totalPages"></param>
    /// <param name="currentPage"></param>
    /// <param name="pageSize"></param>
    /// <param name="hasNextPage"></param>
    /// <param name="hasPreviousPage"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static PagedResult<T> Create(long totalRecords, long totalPages, long currentPage, long pageSize, bool hasNextPage, bool hasPreviousPage, List<T> data)
        => new()
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = pageSize,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            Data = data
        };

    public async static Task<PagedResult<T>> CreateAsync (IQueryable<T> queryable, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        var data = await (await queryable.GetPageAsync(pageSize, pageNumber, cancellationToken)).ToListAsync(cancellationToken);

        var totalRecords = await queryable.LongCountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((decimal)(totalRecords / pageSize));

        var hasNextPage = totalPages > pageNumber;

        var hasPreviousPage = 1 < pageNumber && 1 < totalPages;

        return PagedResult<T>.Create(totalRecords, totalPages, pageNumber, pageSize, hasNextPage, hasPreviousPage, data);
    }
}
