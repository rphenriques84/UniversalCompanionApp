using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.Backend;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Companion.Frontend.Tests.Features.Games;

public sealed class BffGamesEndpointsTests : IAsyncDisposable
{
    private static readonly Uri _gamesUri = new("/bff/games", UriKind.Relative);

    private readonly FakeBackend _backend = new();
    private readonly WebApplicationFactory<Program> _factory;

    public BffGamesEndpointsTests()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var privateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", privateKey)
            .ConfigureTestServices(services => services
                .AddHttpClient<BackendClient>()
                .ConfigurePrimaryHttpMessageHandler(() => _backend)));
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task GetGames_Anonymous_ReturnsBackendGames()
    {
        using var client = _factory.CreateClient();

        var games = await client.GetFromJsonAsync<GameDto[]>(_gamesUri);

        Assert.Equal([new GameDto("ffx", "Final Fantasy X HD Remaster")], games!);
    }

    [Fact]
    public async Task GetGames_ForwardsToBackendGamesWithServiceToken()
    {
        using var client = _factory.CreateClient();

        await client.GetFromJsonAsync<GameDto[]>(_gamesUri);

        Assert.Equal(new Uri("http://backend:8080/games"), _backend.RequestUri);
        var token = new JsonWebToken(_backend.Token);
        Assert.Equal(ServiceScopes.GameDataRead, token.GetClaim(ServiceTokens.ScopeClaim).Value);
    }

    [Fact]
    public async Task GetGames_BrowserAuthorizationHeader_IsNotForwarded()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "browser-supplied-token");

        await client.GetFromJsonAsync<GameDto[]>(_gamesUri);

        Assert.NotEqual("browser-supplied-token", _backend.Token);
    }

    private sealed class FakeBackend : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public string? Token { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Token = request.Headers.Authorization?.Parameter;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[] { new GameDto("ffx", "Final Fantasy X HD Remaster") }),
            });
        }
    }
}
