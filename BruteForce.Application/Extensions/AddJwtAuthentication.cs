
using System.Text;
using BruteForce.Application.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BruteForce.Application.Extensions;

public static class AddJwtAuthenticationExtention
{
    public static void AddJwtAuthentication(this IServiceCollection services, JwtAuthenticationConfigs configs)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configs.SecretKey)),
                    ValidAudience = configs.Audience,
                    ValidIssuer = configs.Issuer,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context => {
                        if (string.IsNullOrEmpty(configs.QueryParamNameOfJwtTokenForSignalR))
                            return Task.CompletedTask;

                        var Token = context.Request.Query[configs.QueryParamNameOfJwtTokenForSignalR];

                        if (!string.IsNullOrEmpty(Token) && context.HttpContext.Request.Path.StartsWithSegments(configs.SignlarHubsCommonStartingPath))
                            context.Token = Token;

                        return Task.CompletedTask;
                    }
                };
            });
    }
}
