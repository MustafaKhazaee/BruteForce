
using System.Text;
using BruteForce.Application.Models;
using Microsoft.IdentityModel.Tokens;
using BruteForce.Application.Services;
using System.IdentityModel.Tokens.Jwt;

namespace BruteForce.Application.Implementations;

public class TokenService : ITokenService
{
    public string GenerateJwtToken(TokenConfigs tokenConfigs)
    {
        SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(tokenConfigs.SecretKey));

        SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtSecurityToken = new(tokenConfigs.Issuer, tokenConfigs.Audience, tokenConfigs.Claims, null, tokenConfigs.ExpirationDate, signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    public async Task<bool> ValidateToken(string token, JwtTokenValidationConfigs configs)
    {
        if (string.IsNullOrEmpty(token))
            throw new Exception("Token is null or empty");

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(configs.SecretKey));
        try
        {
            var result = await jwtSecurityTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                IssuerSigningKey = symmetricSecurityKey,
                ValidAudience = configs.Audience,
                ValidIssuer = configs.Issuer,
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ClockSkew = TimeSpan.Zero
            });

            return result.IsValid;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
