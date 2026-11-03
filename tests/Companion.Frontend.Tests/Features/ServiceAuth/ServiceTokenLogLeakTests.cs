using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Companion.Contracts;
using Companion.Frontend.Features.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

/// <summary>Captures every log record, at Trace level, while the BFF signs tokens and calls the backend.</summary>
public sealed class ServiceTokenLogLeakTests : IAsyncDisposable
{
    private static readonly Uri _gamesUri = new("/bff/games", UriKind.Relative);

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly FakeBackend _backend = new();
    private readonly WebApplicationFactory<Program> _factory;

    public ServiceTokenLogLeakTests() =>
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", Convert.ToBase64String(_key.ExportPkcs8PrivateKey()))
            .UseSetting("Logging:LogLevel:Default", "Trace")
            .UseSetting("Logging:LogLevel:Microsoft.AspNetCore", "Trace")
            .ConfigureLogging(logging => logging.ClearProviders().AddFakeLogging())
            .ConfigureTestServices(services => services
                .AddHttpClient<BackendClient>()
                .ConfigurePrimaryHttpMessageHandler(() => _backend)));

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        _key.Dispose();
    }

    [Fact]
    public async Task GetGames_BackendAcceptsAndRejects_LogsNoSecrets()
    {
        using var client = _factory.CreateClient();

        using var accepted = await client.GetAsync(_gamesUri);
        _backend.Status = HttpStatusCode.Unauthorized;
        using var rejected = await client.GetAsync(_gamesUri);

        var records = _factory.Services.GetFakeLogCollector().GetSnapshot();
        string[] secrets = [Convert.ToBase64String(_key.ExportPkcs8PrivateKey()), .. _backend.Tokens];

        // Proves both calls happened and Trace logging was captured, so the check below has something to read.
        Assert.Equal(2, _backend.Tokens.Count);
        Assert.Contains(records, record => record.Level == LogLevel.Trace);

        // The default HttpClient logs keep the raw Authorization header in structured values.
        Assert.DoesNotContain(records, record => record.Category?.StartsWith("System.Net.Http.HttpClient", StringComparison.Ordinal) == true);
        Assert.All(records, record => Assert.All(secrets, secret =>
            Assert.DoesNotContain(secret, Describe(record), StringComparison.Ordinal)));
    }

    private static string Describe(FakeLogRecord record) =>
        string.Join('\n', [record.Message, record.Exception?.ToString(), .. record.StructuredState?.Select(pair => $"{pair.Key}={pair.Value}") ?? [], .. record.Scopes.Select(scope => scope?.ToString())]);

    private sealed class FakeBackend : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public List<string> Tokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tokens.Add(request.Headers.Authorization!.Parameter!);

            return Task.FromResult(new HttpResponseMessage(Status)
            {
                Content = JsonContent.Create(new[] { new GameDto("ffx", "Final Fantasy X HD Remaster") }),
            });
        }
    }
}
