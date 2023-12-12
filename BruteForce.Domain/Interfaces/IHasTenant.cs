
namespace BruteForce.Domain.Interfaces;

public interface IHasTenant
{
    public int TenantId { set; get; }

    public IHasTenant SetTenantId(int tenantId);
}