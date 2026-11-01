using System.Net;
using System.Security.Cryptography;
using Companion.Frontend.Features.Backend;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Companion.Frontend.Tests.Features.Games;

public sealed class BffGamesErrorTests : IAsyncDisposable
{
    private const string _backendUrl = "http://backend-internal-name:8080";
    private const string _secretDetail = "internal-backend-detail";

    private static readonly Uri _gamesUri = new("/bff/games", UriKind.Relative);

    private readonly FakeBackend _backend = new();
    private readonly WebApplicationFactory<Program> _factory;

    public BffGamesErrorTests()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var privateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("Backend:BaseUrl", _backendUrl)
            .UseSetting("ServiceAuth:PrivateKey", privateKey)
            .ConfigureTestServices(services =>
            {
                services.AddHttpClient<BackendClient>().ConfigurePrimaryHttpMessageHandler(() => _backend);

                // Short waits keep the tests fast.
                services.PostConfigure<HttpStandardResilienceOptions>($"{nameof(BackendClient)}-standard", options =>
                {
                    options.Retry.Delay = TimeSpan.FromMilliseconds(1);
                    options.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(50);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                });
            }));
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetGames_BackendErrorStatus_ReturnsProblemDetails(HttpStatusCode status)
    {
        _backend.Respond = () => new HttpResponseMessage(status) { Content = new StringContent(_secretDetail) };

        await AssertUnavailableAsync();
    }

    [Fact]
    public async Task GetGames_BackendUnreachable_ReturnsProblemDetails()
    {
        _backend.Respond = () => throw new HttpRequestException(_secretDetail);

        await AssertUnavailableAsync();
    }

    [Fact]
    public async Task GetGames_BackendStopsResponding_ReturnsProblemDetails()
    {
        _backend.Hangs = true;

        await AssertUnavailableAsync();
    }

    private async Task AssertUnavailableAsync()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(_gamesUri);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(_secretDetail, body, StringComparison.Ordinal);
        Assert.DoesNotContain("backend-internal-name", body, StringComparison.Ordinal);
    }

    private sealed class FakeBackend : HttpMessageHandler
    {
        public Func<HttpResponseMessage> Respond { get; set; } = () => new HttpResponseMessage(HttpStatusCode.OK);

        public bool Hangs { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Hangs)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            return Respond();
        }
    }
}
