using System.Collections.Concurrent;
using System.Security.Cryptography;
using Companion.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Frontend.Features.ServiceAuth;

public sealed class ServiceTokenIssuer : IDisposable
{
    private static readonly TimeSpan _refreshMargin = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<TokenKey, IssuedToken> _tokens = new();
    private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false };
    private readonly ECDsa _privateKey = ECDsa.Create();
    private readonly SigningCredentials _credentials;
    private readonly TimeSpan _lifetime;
    private readonly TimeProvider _timeProvider;

    public ServiceTokenIssuer(IOptions<ServiceTokenOptions> options, TimeProvider timeProvider)
    {
        _privateKey.ImportPkcs8PrivateKey(Convert.FromBase64String(options.Value.PrivateKey), out _);
        _credentials = new SigningCredentials(new ECDsaSecurityKey(_privateKey), SecurityAlgorithms.EcdsaSha256);
        _lifetime = options.Value.TokenLifetime;
        _timeProvider = timeProvider;
    }

    public string GetToken(string scope, string? userId = null)
    {
        var now = _timeProvider.GetUtcNow();
        var key = new TokenKey(scope, userId);

        if (_tokens.TryGetValue(key, out var cached) && now < cached.ExpiresAt - _refreshMargin)
        {
            return cached.Value;
        }

        RemoveExpired(now);

        var token = Issue(key, now);
        _tokens[key] = token;
        return token.Value;
    }

    public void Dispose() => _privateKey.Dispose();

    private IssuedToken Issue(TokenKey key, DateTimeOffset now)
    {
        var expiresAt = now + _lifetime;
        var claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = key.Scope };

        if (key.UserId is not null)
        {
            claims[ServiceTokens.UserIdClaim] = key.UserId;
        }

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = ServiceTokens.Issuer,
            Audience = ServiceTokens.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = claims,
            SigningCredentials = _credentials,
        });

        return new IssuedToken(token, expiresAt);
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var (key, token) in _tokens)
        {
            if (token.ExpiresAt <= now)
            {
                _tokens.TryRemove(key, out _);
            }
        }
    }

    private readonly record struct TokenKey(string Scope, string? UserId);

    private sealed record IssuedToken(string Value, DateTimeOffset ExpiresAt);
}
