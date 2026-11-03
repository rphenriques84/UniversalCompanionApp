using System.Text.Json;

namespace Companion.E2E;

public sealed class TracingTests
{
    private static readonly Uri _gamesUri = new("http://127.0.0.1:8080/bff/games");

    // Grafana in otel-lgtm allows anonymous access and proxies queries to Tempo.
    private static readonly Uri _frontendTracesUri = new(
        "http://127.0.0.1:3000/api/datasources/proxy/uid/tempo/api/search?q=" +
        Uri.EscapeDataString("{resource.service.name=\"companion-frontend\" && span.url.path=\"/bff/games\"}"));

    private static readonly TimeSpan _waitForTraces = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

    [E2EFact]
    public async Task GetGames_RealStack_FrontendTraceReachesCollector()
    {
        await using var stack = await ComposeStack.StartAsync();
        using var client = new HttpClient();

        using var response = await client.GetAsync(_gamesUri);

        Assert.True(await HasTracesAsync(client), "No frontend trace for /bff/games reached Tempo.");
    }

    private static async Task<bool> HasTracesAsync(HttpClient client)
    {
        var deadline = DateTimeOffset.UtcNow + _waitForTraces;

        while (DateTimeOffset.UtcNow < deadline)
        {
            using var search = JsonDocument.Parse(await client.GetStringAsync(_frontendTracesUri));

            if (search.RootElement.TryGetProperty("traces", out var traces) && traces.GetArrayLength() > 0)
            {
                return true;
            }

            await Task.Delay(_pollInterval);
        }

        return false;
    }
}
