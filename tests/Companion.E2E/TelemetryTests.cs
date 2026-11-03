using System.Text.Json;

namespace Companion.E2E;

public sealed class TelemetryTests
{
    private const string _grafana = "http://127.0.0.1:3000/api/datasources/proxy/uid";

    // Every service token is a JWT, and every JWT starts with this encoded header.
    private const string _jwtStart = "eyJ";

    private static readonly Uri _gamesUri = new("http://127.0.0.1:8080/bff/games");
    private static readonly string[] _services = ["companion-frontend", "companion-backend"];

    // Metrics are exported once a minute, so allow a little more than that.
    private static readonly TimeSpan _waitForTelemetry = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

    [E2EFact]
    public async Task GetGames_RealStack_TelemetryArrivesWithoutSecrets()
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

        var stored = await ReadAllTelemetryAsync(client);
        var keyValues = stack.ReadKeyValues();

        // Proves the scan below reads traces, logs and metrics, not empty answers.
        Assert.Contains("/bff/games", stored, StringComparison.Ordinal);
        Assert.Contains("Application started", stored, StringComparison.Ordinal);
        Assert.Contains("http_server_request_duration_seconds", stored, StringComparison.Ordinal);

        // Assert.False, not DoesNotContain: a failure message must not print the key.
        Assert.NotEmpty(keyValues);
        Assert.All(keyValues, key => Assert.False(stored.Contains(key, StringComparison.Ordinal), "A key value reached Grafana."));
        Assert.False(stored.Contains(_jwtStart, StringComparison.Ordinal), "A token reached Grafana.");
    }

    private static Uri TraceSearch(string service, string path) =>
        new($"{_grafana}/tempo/api/search?q=" +
            Uri.EscapeDataString($"{{resource.service.name=\"{service}\" && span.url.path=\"{path}\"}}"));

    private static Uri LogQuery(string service) =>
        new($"{_grafana}/loki/loki/api/v1/query_range?limit=5000&query=" + Uri.EscapeDataString($"{{service_name=\"{service}\"}}"));

    private static Uri MetricQuery(string service) =>
        new($"{_grafana}/prometheus/api/v1/query?query=" +
            Uri.EscapeDataString($"http_server_request_duration_seconds_count{{job=\"{service}\"}}"));

    private static Uri AllMetricsQuery(string service) =>
        new($"{_grafana}/prometheus/api/v1/query?query=" + Uri.EscapeDataString($"{{job=\"{service}\"}}"));

    /// <summary>Every stored log line, trace and metric series from both services, as raw text.</summary>
    private static async Task<string> ReadAllTelemetryAsync(HttpClient client)
    {
        var stored = new List<string>();

        foreach (var service in _services)
        {
            stored.Add(await client.GetStringAsync(LogQuery(service)));
            stored.Add(await client.GetStringAsync(AllMetricsQuery(service)));

            var search = await client.GetStringAsync(new Uri(
                $"{_grafana}/tempo/api/search?limit=1000&q=" + Uri.EscapeDataString($"{{resource.service.name=\"{service}\"}}")));
            using var traces = JsonDocument.Parse(search);

            foreach (var trace in traces.RootElement.GetProperty("traces").EnumerateArray())
            {
                stored.Add(await client.GetStringAsync(new Uri($"{_grafana}/tempo/api/traces/{trace.GetProperty("traceID").GetString()}")));
            }
        }

        return string.Join('\n', stored);
    }

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
