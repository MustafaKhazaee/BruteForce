namespace BruteForce.Domain.Interfaces;

/// <summary>
/// Implement this interface to make IRepository use soft deleting strategy.
/// </summary>
public interface ISoftDelete
{
    public bool IsDeleted { set; get; }
    public string? DeletedBy { set; get; }
    public DateTime? DeletedDate { set; get; }
}
