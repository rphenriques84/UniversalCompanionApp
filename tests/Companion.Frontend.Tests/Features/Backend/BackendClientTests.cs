using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.Backend;
using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Companion.Frontend.Tests.Features.Backend;

public sealed class BackendClientTests : IDisposable
{
    private readonly FakeBackend _backend = new();
    private readonly ServiceProvider _provider;

    public BackendClientTests() => _provider = CreateProvider("http://backend:8080");

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task GetGamesAsync_CallsBackendGamesPath()
    {
        await GetClient().GetGamesAsync(CancellationToken.None);

        Assert.Equal(new Uri("http://backend:8080/games"), _backend.Request?.RequestUri);
        Assert.Equal(HttpMethod.Get, _backend.Request?.Method);
    }

    [Fact]
    public async Task GetGamesAsync_SendsTokenWithGameDataScope()
    {
        await GetClient().GetGamesAsync(CancellationToken.None);

        var token = new JsonWebToken(_backend.Request?.Headers.Authorization?.Parameter);
        Assert.Equal(ServiceScopes.GameDataRead, token.GetClaim(ServiceTokens.ScopeClaim).Value);
    }

    [Fact]
    public async Task GetGamesAsync_ReturnsGames()
    {
        var games = await GetClient().GetGamesAsync(CancellationToken.None);

        Assert.Equal([new GameDto("ffx", "Final Fantasy X HD Remaster")], games);
    }

    [Fact]
    public async Task GetGamesAsync_ErrorStatus_Throws()
    {
        _backend.Response = () => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() => GetClient().GetGamesAsync(CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void BackendOptions_InvalidBaseUrl_Fails(string baseUrl)
    {
        using var provider = CreateProvider(baseUrl);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<BackendOptions>>().Value);
    }

    private BackendClient GetClient() => _provider.GetRequiredService<BackendClient>();

    private ServiceProvider CreateProvider(string baseUrl)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backend:BaseUrl"] = baseUrl,
                ["ServiceAuth:PrivateKey"] = Convert.ToBase64String(key.ExportPkcs8PrivateKey()),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddServiceTokens(configuration);
        services.AddBackendClient(configuration).ConfigurePrimaryHttpMessageHandler(() => _backend);

        return services.BuildServiceProvider();
    }

    private sealed class FakeBackend : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public Func<HttpResponseMessage> Response { get; set; } = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { new GameDto("ffx", "Final Fantasy X HD Remaster") }),
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(Response());
        }
    }
}
