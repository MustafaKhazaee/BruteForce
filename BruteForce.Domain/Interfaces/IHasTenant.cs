
namespace BruteForce.Domain.Interfaces;

public interface IHasTenant
{
    public int TenantId { set; get; }
    public string? TenantName { set; get; }

    public IHasTenant SetTenantId(int tenantId)
    {
        TenantId = tenantId;
        return this;
    }

    public IHasTenant SetTenantName(string? tenantName)
    {
        TenantName = tenantName;
        return this;
    }
}