using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Companion.Backend.Api.Features.ServiceAuth;
using Companion.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Backend.Tests.Features.ServiceAuth;

public sealed class ServiceAuthTests : IAsyncLifetime
{
    private static readonly string _scopedPath = "/scoped";
    private static readonly string _defaultPath = "/default";

    private readonly ECDsa _currentKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly ECDsa _previousKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly ECDsa _unknownKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ServiceAuth:CurrentPublicKey"] = Convert.ToBase64String(_currentKey.ExportSubjectPublicKeyInfo()),
            ["ServiceAuth:PreviousPublicKey"] = Convert.ToBase64String(_previousKey.ExportSubjectPublicKeyInfo()),
        });
        builder.Services.AddServiceAuth(builder.Configuration);

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapGet(_scopedPath, () => "ok").RequireAuthorization(ServiceScopes.GameDataRead);
        _app.MapGet(_defaultPath, () => "ok");

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        _currentKey.Dispose();
        _previousKey.Dispose();
        _unknownKey.Dispose();
    }

    [Fact]
    public async Task Request_ValidToken_IsAccepted()
    {
        var status = await GetStatusAsync(_scopedPath, CreateToken(_currentKey));

        Assert.Equal(HttpStatusCode.OK, status);
    }

    [Fact]
    public async Task Request_PreviousKeyToken_IsAccepted()
    {
        var status = await GetStatusAsync(_scopedPath, CreateToken(_previousKey));

        Assert.Equal(HttpStatusCode.OK, status);
    }

    [Fact]
    public async Task Request_MultipleScopes_IsAccepted()
    {
        var token = CreateToken(_currentKey, scope: $"other.scope {ServiceScopes.GameDataRead}");

        Assert.Equal(HttpStatusCode.OK, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_MissingToken_IsRejected()
    {
        var status = await GetStatusAsync(_scopedPath, token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Request_EndpointWithoutPolicy_RequiresToken()
    {
        var status = await GetStatusAsync(_defaultPath, token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Request_ExpiredToken_IsRejected()
    {
        var token = CreateToken(_currentKey, expiresAt: DateTime.UtcNow.AddMinutes(-5));

        Assert.Equal(HttpStatusCode.Unauthorized, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_WrongAudience_IsRejected()
    {
        var token = CreateToken(_currentKey, audience: "another-service");

        Assert.Equal(HttpStatusCode.Unauthorized, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_WrongIssuer_IsRejected()
    {
        var token = CreateToken(_currentKey, issuer: "another-service");

        Assert.Equal(HttpStatusCode.Unauthorized, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_UnknownSigningKey_IsRejected()
    {
        var status = await GetStatusAsync(_scopedPath, CreateToken(_unknownKey));

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Request_WrongScope_IsForbidden()
    {
        var token = CreateToken(_currentKey, scope: "other.scope");

        Assert.Equal(HttpStatusCode.Forbidden, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_ScopeWithSamePrefix_IsForbidden()
    {
        var token = CreateToken(_currentKey, scope: $"{ServiceScopes.GameDataRead}.extra");

        Assert.Equal(HttpStatusCode.Forbidden, await GetStatusAsync(_scopedPath, token));
    }

    [Fact]
    public async Task Request_UnsignedToken_IsRejected()
    {
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = ServiceTokens.Issuer,
            Audience = ServiceTokens.Audience,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = ServiceScopes.GameDataRead },
        });

        Assert.Equal(HttpStatusCode.Unauthorized, await GetStatusAsync(_scopedPath, token));
    }

    private static string CreateToken(
        ECDsa key,
        string issuer = ServiceTokens.Issuer,
        string audience = ServiceTokens.Audience,
        string scope = ServiceScopes.GameDataRead,
        DateTime? expiresAt = null)
    {
        var expires = expiresAt ?? DateTime.UtcNow.AddMinutes(5);

        return new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            IssuedAt = expires.AddMinutes(-5),
            NotBefore = expires.AddMinutes(-5),
            Expires = expires,
            Claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = scope },
            SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key), SecurityAlgorithms.EcdsaSha256),
        });
    }

    private async Task<HttpStatusCode> GetStatusAsync(string path, string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await _client.SendAsync(request);
        return response.StatusCode;
    }
}
