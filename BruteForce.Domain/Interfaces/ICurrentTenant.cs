
namespace BruteForce.Domain.Interfaces;

public interface ICurrentTenant
{
    int GetTenantId();

    string GetTenantName();
}