namespace BruteForce.Domain.Interfaces;

/// <summary>
/// Implement this interface to make IRepository use soft deleting strategy.
/// </summary>
public interface ISoftDelete
{
    public bool IsDeleted { set; get; }
    public string? DeletedBy { set; get; }
    public DateTime? DeletedDate { set; get; }

    public ISoftDelete SetIsDeleted (bool isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }

    public ISoftDelete SetDeletedBy (string deletedBy)
    {
        DeletedBy = deletedBy;
        return this;
    }

    public ISoftDelete SetDeletedDate (DateTime deletedDate)
    {
        DeletedDate = deletedDate;
        return this;
    }
}
