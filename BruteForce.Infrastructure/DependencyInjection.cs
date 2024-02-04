
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using BruteForce.Infrastructure.Enums;
using BruteForce.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BruteForce.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Please provide your DbContext class in type parameter to register the DbContext and this method
    /// will provide the implementation for IRepository. Before calling this method
    /// you need to provide your own implementation for BruteForce.Domain.Interfaces.ICurrentUser
    /// to enable auditing of your entities.
    /// </summary>
    public static IServiceCollection AddInfrastructure<ApplicationDbContext> (this IServiceCollection services,
        DatabaseProviders DatabaseType, string connectionString) where ApplicationDbContext : DbContext, IApplicationDbContext
    {
        return services.AddDbContextPool<IApplicationDbContext, ApplicationDbContext>(config =>
            {
                if (DatabaseType == DatabaseProviders.SQLServer)
                    config.UseSqlServer(connectionString);
                else if (DatabaseType == DatabaseProviders.PostgreSQL)
                    config.UseNpgsql(connectionString);

                config.EnableDetailedErrors();
            }
        )
        .AddScoped<IUnitOfWork, UnitOfWork>()
        .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
        .AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }
}
