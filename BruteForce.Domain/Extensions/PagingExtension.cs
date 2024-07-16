
namespace BruteForce.Domain.Extensions;

public static class PagingExtension
{
    /// <summary>
    /// You need to order it for paging to work properly!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async static Task<IQueryable<T>> GetPageAsync<T> (this IQueryable<T> queryable, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new Exception("PageNumber should be a positive number!");

        if (pageSize < 1)
            throw new Exception("PageSize should be a positive number!");

        var skip = (int)Math.Ceiling((decimal)(pageSize * (pageNumber - 1)));

        return queryable.Skip(skip).Take(pageSize);
    }
}
