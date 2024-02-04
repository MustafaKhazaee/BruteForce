
using BruteForce.Application.Models;

namespace BruteForce.Application.Services;

public interface ICookieAuthenticationService
{
    Task CookieSignIn(CookieSignInConfigs configs);

    Task CookieSignOut();
}
