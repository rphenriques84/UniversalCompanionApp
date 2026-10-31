using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

public sealed class ServiceTokenIssuerTests : IDisposable
{
    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ServiceTokenIssuer _issuer;

    public ServiceTokenIssuerTests()
    {
        var options = new ServiceTokenOptions { PrivateKey = Convert.ToBase64String(_key.ExportPkcs8PrivateKey()) };
        _issuer = new ServiceTokenIssuer(Options.Create(options), _time);
    }

    public void Dispose()
    {
        _issuer.Dispose();
        _key.Dispose();
    }

    [Fact]
    public void GetToken_SetsIssuerAudienceAndScope()
    {
        var token = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead));

        Assert.Equal(ServiceTokens.Issuer, token.Issuer);
        Assert.Equal(ServiceTokens.Audience, Assert.Single(token.Audiences));
        Assert.Equal(ServiceScopes.GameDataRead, token.GetClaim(ServiceTokens.ScopeClaim).Value);
        Assert.Equal(SecurityAlgorithms.EcdsaSha256, token.Alg);
    }

    [Fact]
    public void GetToken_ExpiresWithinFiveMinutes()
    {
        var token = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead));

        Assert.Equal(_time.GetUtcNow().UtcDateTime.AddMinutes(5), token.ValidTo);
    }

    [Fact]
    public async Task GetToken_SignatureMatchesPublicKey()
    {
        var parameters = new TokenValidationParameters
        {
            ValidIssuer = ServiceTokens.Issuer,
            ValidAudience = ServiceTokens.Audience,
            IssuerSigningKey = new ECDsaSecurityKey(_key),
            ValidateLifetime = false,
        };

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(_issuer.GetToken(ServiceScopes.GameDataRead), parameters);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void GetToken_WithoutUserId_HasNoUidClaim()
    {
        var token = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead));

        Assert.False(token.TryGetClaim(ServiceTokens.UserIdClaim, out _));
    }

    [Fact]
    public void GetToken_WithUserId_SetsUidClaim()
    {
        var token = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead, "user-1"));

        Assert.Equal("user-1", token.GetClaim(ServiceTokens.UserIdClaim).Value);
    }

    [Fact]
    public void GetToken_BeforeRefreshMargin_ReturnsCachedToken()
    {
        var first = _issuer.GetToken(ServiceScopes.GameDataRead);

        _time.Advance(TimeSpan.FromMinutes(3));

        Assert.Equal(first, _issuer.GetToken(ServiceScopes.GameDataRead));
    }

    [Fact]
    public void GetToken_InsideRefreshMargin_ReturnsNewToken()
    {
        var first = _issuer.GetToken(ServiceScopes.GameDataRead);

        _time.Advance(TimeSpan.FromMinutes(4));

        Assert.NotEqual(first, _issuer.GetToken(ServiceScopes.GameDataRead));
    }

    [Fact]
    public void GetToken_DifferentUsers_DoNotShareTokens()
    {
        var first = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead, "user-1"));
        var second = new JsonWebToken(_issuer.GetToken(ServiceScopes.GameDataRead, "user-2"));

        Assert.NotEqual(first.GetClaim(ServiceTokens.UserIdClaim).Value, second.GetClaim(ServiceTokens.UserIdClaim).Value);
    }
}
