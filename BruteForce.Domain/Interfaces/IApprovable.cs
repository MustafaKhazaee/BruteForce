
namespace BruteForce.Domain.Interfaces;

public interface IApprovable
{
    bool IsApproved { get; set; }
    string? ApprovedBy { get; set; }
    DateTime? ApprovedDate { get; set; }

    public IApprovable Approve(string? approvedBy, DateTime? approvedDate)
    {
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedDate = approvedDate;
        return this;
    }
}
