
using BruteForce.Application.Services;
using BruteForce.Application.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BruteForce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication (this IServiceCollection services)
    {
        return services.AddScoped<ICookieAuthenticationService, CookieAuthenticationService>()
                       .AddScoped<ITokenService, TokenService>()
                       .AddScoped<IImageService, ImageService>();
    }
}
