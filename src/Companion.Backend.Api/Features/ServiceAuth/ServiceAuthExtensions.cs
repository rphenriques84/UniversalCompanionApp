using System.Security.Cryptography;
using Companion.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Backend.Api.Features.ServiceAuth;

public static class ServiceAuthExtensions
{
    private static readonly TimeSpan _clockSkew = TimeSpan.FromSeconds(30);

    public static IServiceCollection AddServiceAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceAuthOptions>()
            .Bind(configuration.GetSection(ServiceAuthOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ServiceAuthOptions>, ServiceAuthOptionsValidator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ServiceAuthOptions>>((bearer, auth) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = CreateValidationParameters(auth.Value);
            });

        // Deny by default: endpoints without an explicit policy still need a valid token.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(ServiceScopes.GameDataRead, policy => policy.RequireAuthenticatedUser().RequireScope(ServiceScopes.GameDataRead));

        return services;
    }

    private static TokenValidationParameters CreateValidationParameters(ServiceAuthOptions options) => new()
    {
        ValidIssuer = ServiceTokens.Issuer,
        ValidAudience = ServiceTokens.Audience,
        ValidAlgorithms = [SecurityAlgorithms.EcdsaSha256],
        IssuerSigningKeys = CreateSigningKeys(options),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ClockSkew = _clockSkew,
    };

    private static List<SecurityKey> CreateSigningKeys(ServiceAuthOptions options)
    {
        var keys = new List<SecurityKey> { CreateSigningKey(options.CurrentPublicKey) };

        if (!string.IsNullOrEmpty(options.PreviousPublicKey))
        {
            keys.Add(CreateSigningKey(options.PreviousPublicKey));
        }

        return keys;
    }

    private static ECDsaSecurityKey CreateSigningKey(string publicKey)
    {
        var key = ECDsa.Create();
        key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
        return new ECDsaSecurityKey(key);
    }

    private static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policy, string scope) =>
        policy.RequireAssertion(context => context.User
            .FindAll(ServiceTokens.ScopeClaim)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains(scope, StringComparer.Ordinal));
}
