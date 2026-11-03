using System.Text.Json;

namespace Companion.E2E;

public sealed class TelemetryTests
{
    private const string _grafana = "http://127.0.0.1:3000/api/datasources/proxy/uid";

    private static readonly Uri _gamesUri = new("http://127.0.0.1:8080/bff/games");

    // Metrics are exported once a minute, so allow a little more than that.
    private static readonly TimeSpan _waitForTelemetry = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

    [E2EFact]
    public async Task GetGames_RealStack_TelemetryReachesCollector()
    {
        await using var stack = await ComposeStack.StartAsync();
        using var client = new HttpClient();

        using var response = await client.GetAsync(_gamesUri);

        // Grafana in otel-lgtm allows anonymous access and proxies queries to Tempo, Loki and Prometheus.
        foreach (var (service, path) in new[] { ("companion-frontend", "/bff/games"), ("companion-backend", "/games") })
        {
            Assert.True(await HasResultsAsync(client, TraceSearch(service, path), "traces"), $"No trace from {service}.");
            Assert.True(await HasResultsAsync(client, LogQuery(service), "result"), $"No logs from {service}.");
            Assert.True(await HasResultsAsync(client, MetricQuery(service), "result"), $"No metrics from {service}.");
        }
    }

    private static Uri TraceSearch(string service, string path) =>
        new($"{_grafana}/tempo/api/search?q=" +
            Uri.EscapeDataString($"{{resource.service.name=\"{service}\" && span.url.path=\"{path}\"}}"));

    private static Uri LogQuery(string service) =>
        new($"{_grafana}/loki/loki/api/v1/query_range?query=" + Uri.EscapeDataString($"{{service_name=\"{service}\"}}"));

    private static Uri MetricQuery(string service) =>
        new($"{_grafana}/prometheus/api/v1/query?query=" +
            Uri.EscapeDataString($"http_server_request_duration_seconds_count{{job=\"{service}\"}}"));

    private static async Task<bool> HasResultsAsync(HttpClient client, Uri queryUri, string arrayName)
    {
        var deadline = DateTimeOffset.UtcNow + _waitForTelemetry;

        while (DateTimeOffset.UtcNow < deadline)
        {
            using var answer = JsonDocument.Parse(await client.GetStringAsync(queryUri));
            var root = answer.RootElement.TryGetProperty("data", out var data) ? data : answer.RootElement;

            if (root.TryGetProperty(arrayName, out var results) && results.GetArrayLength() > 0)
            {
                return true;
            }

            await Task.Delay(_pollInterval);
        }

        return false;
    }
}
