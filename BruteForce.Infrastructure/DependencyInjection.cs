
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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
    public static IServiceCollection AddInfrastructure<ApplicationDbContext> (this IServiceCollection services, string connectionString) where ApplicationDbContext : DbContext, IApplicationDbContext
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(config =>
            {
                config.UseNpgsql(connectionString);
                config.EnableDetailedErrors();
            }
            , ServiceLifetime.Scoped
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
