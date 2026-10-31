using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

public sealed class ServiceTokenHandlerTests : IDisposable
{
    private readonly ServiceTokenIssuer _issuer;
    private readonly CapturingHandler _backend = new();
    private readonly HttpClient _client;

    public ServiceTokenHandlerTests()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var options = new ServiceTokenOptions { PrivateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey()) };

        _issuer = new ServiceTokenIssuer(Options.Create(options), TimeProvider.System);
        _client = new HttpClient(new ServiceTokenHandler(_issuer) { InnerHandler = _backend })
        {
            BaseAddress = new Uri("http://backend"),
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _issuer.Dispose();
    }

    [Fact]
    public async Task SendAsync_WithScope_AddsBearerToken()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/games");
        request.Options.Set(ServiceTokenHandler.Scope, ServiceScopes.GameDataRead);

        using var response = await _client.SendAsync(request, CancellationToken.None);

        Assert.Equal("Bearer", _backend.Authorization?.Scheme);
        var token = new JsonWebToken(_backend.Authorization?.Parameter);
        Assert.Equal(ServiceScopes.GameDataRead, token.GetClaim(ServiceTokens.ScopeClaim).Value);
    }

    [Fact]
    public async Task SendAsync_WithUserId_AddsUidClaim()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/games");
        request.Options.Set(ServiceTokenHandler.Scope, ServiceScopes.GameDataRead);
        request.Options.Set(ServiceTokenHandler.UserId, "user-1");

        using var response = await _client.SendAsync(request, CancellationToken.None);

        var token = new JsonWebToken(_backend.Authorization?.Parameter);
        Assert.Equal("user-1", token.GetClaim(ServiceTokens.UserIdClaim).Value);
    }

    [Fact]
    public async Task SendAsync_WithoutScope_Throws()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/games");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.SendAsync(request, CancellationToken.None));
        Assert.Null(_backend.Authorization);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

}
