
using BruteForce.Domain.Interfaces;

namespace BruteForce.Domain.Entities;

public interface IAuditCreation
{
    public string? CreatedBy { set; get; }
    public DateTime? CreatedDate { set; get; }

    public IAuditCreation SetCreatedBy (string createdBy)
    {
        CreatedBy = createdBy;
        return this;
    }

    public IAuditCreation SetCreatedDate (DateTime createdDate)
    {
        CreatedDate = createdDate;
        return this;
    }
}
