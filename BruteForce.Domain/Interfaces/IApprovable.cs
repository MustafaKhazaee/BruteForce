
using BruteForce.Domain.Enums;

namespace BruteForce.Domain.Interfaces;

public interface IApprovable
{
    RecordStatus RecordStatus { get; set; }
    string? ApprovedBy { get; set; }
    DateTime? ApprovedDate { get; set; }

    public IApprovable Approve(string? approvedBy, DateTime? approvedDate)
    {
        RecordStatus = RecordStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedDate = approvedDate;
        return this;
    }

    public IApprovable Reject(string? approvedBy, DateTime? approvedDate)
    {
        RecordStatus = RecordStatus.Rejected;
        ApprovedBy = approvedBy;
        ApprovedDate = approvedDate;
        return this;
    }

    public IApprovable SetRecordStatus(RecordStatus recordStatus)
    {
        RecordStatus = recordStatus;
        return this;
    }
}
