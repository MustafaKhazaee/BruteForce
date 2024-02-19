
using Microsoft.AspNetCore.SignalR;
using BruteForce.RealTime.Interfaces;
using BruteForce.RealTime.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BruteForce.RealTime;

public static class DependencyInjection
{
    public static IServiceCollection AddRealTime (this IServiceCollection services)
    {
        services.AddScoped<IRealTimeUserStorage, RealTimeUserStorage>();
        services.AddSingleton<IUserIdProvider, UserIdProvider>();
        services.AddSignalR(o =>
                {
                    o.EnableDetailedErrors = true;
                });
        return services;
    }
}
