
using BruteForce.Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BruteForce.Application.Extensions
{
    public static class AddCookieAuthenticationExtension
    {
        public static void AddCookieAuthentication(this IServiceCollection services, CookieAuthenticationConfigs configs)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.ExpireTimeSpan = configs.ExpireTimeSpan;
                    options.SlidingExpiration = configs.SlidingExpiration;
                    options.AccessDeniedPath = configs.AccessDeniedPath;
                    options.LoginPath = configs.LoginPath;
                    options.LogoutPath = configs.LogoutPath;
                });
        }
    }
}
