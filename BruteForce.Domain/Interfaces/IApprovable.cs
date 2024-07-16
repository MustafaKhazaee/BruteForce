
using BruteForce.Domain.Enums;

namespace BruteForce.Domain.Interfaces;

public interface IApprovable
{
    RecordStatus RecordStatus { get; set; }
    string? Actor { get; set; }
    DateTime? ActionDate { get; set; }

    public IApprovable Approve(string? actor, DateTime? actionDate)
    {
        RecordStatus = RecordStatus.APPROVED;
        Actor = actor;
        ActionDate = actionDate;
        return this;
    }

    public IApprovable Reject(string? actor, DateTime? actionDate)
    {
        RecordStatus = RecordStatus.REJECTED;
        Actor = actor;
        ActionDate = actionDate;
        return this;
    }

    public IApprovable SetRecordStatus(RecordStatus recordStatus, string? actor, DateTime? actionDate)
    {
        RecordStatus = recordStatus;
        Actor = actor;
        ActionDate = actionDate;
        return this;
    }

    public IApprovable SetRecordStatus(RecordStatus recordStatus)
    {
        RecordStatus = recordStatus;
        return this;
    }
}
