
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using BruteForce.Application.Models;
using BruteForce.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BruteForce.Application.Implementations;

public class CookieAuthenticationService (IHttpContextAccessor httpContextAccessor) : ICookieAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task CookieSignIn(CookieSignInConfigs configs)
    {
        ClaimsIdentity claimsIdentity = new (configs.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

        AuthenticationProperties authenticationProperties = new ()
        {
            IsPersistent = configs.IsPersistent,
            IssuedUtc = configs.IssuedUtc,
            ExpiresUtc = configs.ExpiresUtc,
            RedirectUri = configs.RedirectUrl,
            AllowRefresh = configs.AllowRefresh
        };

        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);
    }

    public async Task CookieSignOut() => await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
