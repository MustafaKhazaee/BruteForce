
using BruteForce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using BruteForce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BruteForce.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Please provide your DbContext class in type parameter to register the DbContext and this method
    /// will provide the implementation for IRepository. Before calling this method
    /// you need to provide your own implementation for BruteForce.Domain.Interfaces.ICurrentUser
    /// to enable auditing of your entities. Name your connection string as "DefaultConnection".
    /// </summary>
    public static IServiceCollection AddInfrastructure<ApplicationDbContext> (this IServiceCollection services, IConfiguration configuration) where ApplicationDbContext : DbContext, IApplicationDbContext
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(config =>
            {
                config.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                config.EnableDetailedErrors();
            }
            , ServiceLifetime.Scoped
        );

        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
