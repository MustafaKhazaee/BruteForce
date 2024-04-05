
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

    public PagedResult() { }

    public PagedResult(long totalRecords, long totalPages, long currentPage, long pageSize, bool hasNextPage, bool hasPreviousPage, List<T> data)
    {
        TotalRecords = totalRecords;
        TotalPages = totalPages;
        CurrentPage = currentPage;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
        Data = data;
    }
}
