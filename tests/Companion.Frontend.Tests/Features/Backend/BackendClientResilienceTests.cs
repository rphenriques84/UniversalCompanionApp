using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.Backend;
using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.JsonWebTokens;
using Polly.Timeout;

namespace Companion.Frontend.Tests.Features.Backend;

public sealed class BackendClientResilienceTests : IDisposable
{
    private const int _maxAttempts = 3;

    private readonly FakeBackend _backend = new();
    private readonly ServiceProvider _provider;

    public BackendClientResilienceTests() => _provider = CreateProvider();

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task GetGamesAsync_TemporaryFailure_RetriesAndSucceeds()
    {
        _backend.Statuses.Enqueue(HttpStatusCode.ServiceUnavailable);

        var games = await GetClient().GetGamesAsync(CancellationToken.None);

        Assert.Single(games);
        Assert.Equal(2, _backend.Attempts);
    }

    [Fact]
    public async Task GetGamesAsync_Retry_SendsTokenOnEveryAttempt()
    {
        _backend.Statuses.Enqueue(HttpStatusCode.ServiceUnavailable);

        await GetClient().GetGamesAsync(CancellationToken.None);

        Assert.Equal(2, _backend.Tokens.Count);
        Assert.All(_backend.Tokens, token =>
            Assert.Equal(ServiceScopes.GameDataRead, new JsonWebToken(token).GetClaim(ServiceTokens.ScopeClaim).Value));
    }

    [Fact]
    public async Task GetGamesAsync_ConstantFailure_GivesUpAfterMaxAttempts()
    {
        _backend.DefaultStatus = HttpStatusCode.InternalServerError;

        await Assert.ThrowsAsync<HttpRequestException>(() => GetClient().GetGamesAsync(CancellationToken.None));

        Assert.Equal(_maxAttempts, _backend.Attempts);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetGamesAsync_AuthFailure_IsNotRetried(HttpStatusCode status)
    {
        _backend.DefaultStatus = status;

        await Assert.ThrowsAsync<HttpRequestException>(() => GetClient().GetGamesAsync(CancellationToken.None));

        Assert.Equal(1, _backend.Attempts);
    }

    [Fact]
    public async Task Send_Post_IsNotRetried()
    {
        _backend.DefaultStatus = HttpStatusCode.ServiceUnavailable;
        using var client = _provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(BackendClient));
        using var request = new HttpRequestMessage(HttpMethod.Post, "games");
        request.Options.Set(ServiceTokenHandler.Scope, ServiceScopes.GameDataRead);

        using var response = await client.SendAsync(request);

        Assert.Equal(1, _backend.Attempts);
    }

    [Fact]
    public async Task GetGamesAsync_BackendStopsResponding_TimesOut()
    {
        _backend.Hangs = true;

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => GetClient().GetGamesAsync(CancellationToken.None));

        Assert.Equal(_maxAttempts, _backend.Attempts);
    }

    private BackendClient GetClient() => _provider.GetRequiredService<BackendClient>();

    private ServiceProvider CreateProvider()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backend:BaseUrl"] = "http://backend:8080",
                ["ServiceAuth:PrivateKey"] = Convert.ToBase64String(key.ExportPkcs8PrivateKey()),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddServiceTokens(configuration);
        services.AddBackendClient(configuration).ConfigurePrimaryHttpMessageHandler(() => _backend);

        // Keeps the retry count and rules, but makes the waits short so tests stay fast.
        services.PostConfigure<HttpStandardResilienceOptions>($"{nameof(BackendClient)}-standard", options =>
        {
            options.Retry.Delay = TimeSpan.FromMilliseconds(1);
            options.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(50);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
        });

        return services.BuildServiceProvider();
    }

    private sealed class FakeBackend : HttpMessageHandler
    {
        public Queue<HttpStatusCode> Statuses { get; } = new();

        public List<string?> Tokens { get; } = [];

        public HttpStatusCode DefaultStatus { get; set; } = HttpStatusCode.OK;

        public bool Hangs { get; set; }

        public int Attempts { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Attempts++;
            Tokens.Add(request.Headers.Authorization?.Parameter);

            if (Hangs)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            var status = Statuses.TryDequeue(out var next) ? next : DefaultStatus;

            return new HttpResponseMessage(status)
            {
                Content = JsonContent.Create(new[] { new GameDto("ffx", "Final Fantasy X HD Remaster") }),
            };
        }
    }
}
