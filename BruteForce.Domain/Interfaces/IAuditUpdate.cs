
namespace BruteForce.Domain.Interfaces;

public interface IAuditUpdate
{
    public string? UpdatedBy { set; get; }
    public DateTime? UpdatedDate { set; get; }

    public IAuditUpdate SetUpdatedBy(string? updatedBy)
    {
        UpdatedBy = updatedBy;
        return this;
    }

    public IAuditUpdate SetUpdatedDate(DateTime? updatedDate)
    {
        UpdatedDate = updatedDate;
        return this;
    }
}
