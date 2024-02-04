

using BruteForce.Application.Models;

namespace BruteForce.Application.Services;

public interface ITokenService
{
    string GenerateJwtToken(TokenConfigs tokenConfigs);

    Task<bool> ValidateToken(string token, JwtTokenValidationConfigs configs);
}
