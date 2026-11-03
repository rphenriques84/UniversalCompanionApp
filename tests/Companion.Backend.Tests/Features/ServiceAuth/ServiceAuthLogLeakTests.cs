using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Companion.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Backend.Tests.Features.ServiceAuth;

/// <summary>Captures every log record, at Trace level, while the backend accepts and rejects tokens.</summary>
public sealed class ServiceAuthLogLeakTests : IAsyncDisposable
{
    private const string _malformedToken = "not-a-jwt-but-a-secret-looking-value";

    private static readonly Uri _gamesUri = new("/games", UriKind.Relative);

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly ECDsa _otherKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly WebApplicationFactory<Program> _factory;

    public ServiceAuthLogLeakTests() =>
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("ServiceAuth:CurrentPublicKey", Convert.ToBase64String(_key.ExportSubjectPublicKeyInfo()))
            .UseSetting("Logging:LogLevel:Default", "Trace")
            .UseSetting("Logging:LogLevel:Microsoft.AspNetCore", "Trace")
            .ConfigureLogging(logging => logging.ClearProviders().AddFakeLogging()));

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        _key.Dispose();
        _otherKey.Dispose();
    }

    [Fact]
    public async Task GetGames_ValidAndRejectedTokens_LogsNoSecrets()
    {
        var now = DateTime.UtcNow;
        var valid = CreateToken(_key, ServiceScopes.GameDataRead, now);
        var wrongScope = CreateToken(_key, "other.scope", now);
        var wrongKey = CreateToken(_otherKey, ServiceScopes.GameDataRead, now);
        var expired = CreateToken(_key, ServiceScopes.GameDataRead, now.AddMinutes(-20));
        using var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, await GetGamesAsync(client, valid));
        Assert.Equal(HttpStatusCode.Forbidden, await GetGamesAsync(client, wrongScope));
        Assert.Equal(HttpStatusCode.Unauthorized, await GetGamesAsync(client, wrongKey));
        Assert.Equal(HttpStatusCode.Unauthorized, await GetGamesAsync(client, expired));
        Assert.Equal(HttpStatusCode.Unauthorized, await GetGamesAsync(client, _malformedToken));

        var records = _factory.Services.GetFakeLogCollector().GetSnapshot();
        string[] secrets =
        [
            Convert.ToBase64String(_key.ExportSubjectPublicKeyInfo()),
            Convert.ToBase64String(_key.ExportPkcs8PrivateKey()),
            Convert.ToBase64String(_otherKey.ExportPkcs8PrivateKey()),
            valid, wrongScope, wrongKey, expired, _malformedToken,
        ];

        // Proves the rejections were logged, so the check below has something to read.
        Assert.Contains(records, record => record.Category == typeof(JwtBearerHandler).FullName);
        Assert.All(records, record => Assert.All(secrets, secret =>
            Assert.DoesNotContain(secret, Describe(record), StringComparison.Ordinal)));
    }

    private static async Task<HttpStatusCode> GetGamesAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _gamesUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    private static string CreateToken(ECDsa key, string scope, DateTime issuedAt) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = ServiceTokens.Issuer,
        Audience = ServiceTokens.Audience,
        IssuedAt = issuedAt,
        NotBefore = issuedAt,
        Expires = issuedAt.AddMinutes(5),
        Claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = scope },
        SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key), SecurityAlgorithms.EcdsaSha256),
    });

    private static string Describe(FakeLogRecord record) =>
        string.Join('\n', [record.Message, record.Exception?.ToString(), .. record.StructuredState?.Select(pair => $"{pair.Key}={pair.Value}") ?? [], .. record.Scopes.Select(scope => scope?.ToString())]);
}
