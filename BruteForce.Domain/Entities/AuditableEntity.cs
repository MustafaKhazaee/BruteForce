
namespace BruteForce.Domain.Entities;

/// <summary>
/// Inheriting from this class will add fields to keep track of changes in your entities. 
/// </summary>
/// <typeparam name="T">Specifies the type of primary key (usually int or long)</typeparam>
public abstract class AuditableEntity<TKey> : AggregateRoot<TKey> where TKey: IComparable
{
    public string? CreatedBy { set; get; }
    public DateTime? CreatedDate { set; get; }
    public string? ModifiedBy { set; get; }
    public DateTime? ModifiedDate { set; get; }

    public AuditableEntity<TKey> SetCreatedBy (string createdBy)
    {
        CreatedBy = createdBy;
        return this;
    }

    public AuditableEntity<TKey> SetCreatedDate (DateTime createdDate)
    {
        CreatedDate = createdDate;
        return this;
    }

    public AuditableEntity<TKey> SetModifiedBy (string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        return this;
    }

    public AuditableEntity<TKey> SetModifiedDate(DateTime modifiedDate)
    {
        ModifiedDate = modifiedDate;
        return this;
    }
}
