
using System.Security.Claims;

namespace BruteForce.Application.Models;

public sealed record TokenConfigs(string SecretKey, string Issuer, string Audience, IEnumerable<Claim> Claims, DateTime ExpirationDate);

public sealed record CookieSignInConfigs(IEnumerable<Claim> Claims, bool IsPersistent, string RedirectUrl, DateTime ExpiresUtc, DateTime IssuedUtc = default, bool AllowRefresh = default);

public sealed record ImageConfigs();

public sealed record JwtTokenValidationConfigs(string SecretKey, string Issuer, string Audience);

public sealed record CookieAuthenticationConfigs(TimeSpan ExpireTimeSpan, bool SlidingExpiration, string LoginPath, string LogoutPath, string AccessDeniedPath);

/// <summary>
/// Provide null for last 2 params if you are not using SignalR
/// </summary>
public sealed record JwtAuthenticationConfigs(string SecretKey, string Issuer, string Audience, 
    string? QueryParamNameOfJwtTokenForSignalR = null, string? SignlarHubsCommonStartingPath = null);// example: "/api/hub/"