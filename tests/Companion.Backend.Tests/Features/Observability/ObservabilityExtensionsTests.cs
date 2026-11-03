using System.Diagnostics;
using Companion.Backend.Api.Features.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Companion.Backend.Tests.Features.Observability;

public sealed class ObservabilityExtensionsTests
{
    // The ActivitySource that ASP.NET Core starts request traces from.
    private const string _aspNetCoreSource = "Microsoft.AspNetCore";

    [Fact]
    public void AddObservability_WithEndpoint_RegistersTracesMetricsAndLogs()
    {
        using var provider = CreateProvider("http://otel-lgtm:4317");

        Assert.NotNull(provider.GetService<TracerProvider>());
        Assert.NotNull(provider.GetService<MeterProvider>());
        Assert.Contains(provider.GetServices<ILoggerProvider>(), logger => logger is OpenTelemetryLoggerProvider);
    }

    [Fact]
    public void AddObservability_NoEndpoint_SendsNothing()
    {
        using var provider = CreateProvider(null);

        Assert.Null(provider.GetService<TracerProvider>());
        Assert.Null(provider.GetService<MeterProvider>());
        Assert.DoesNotContain(provider.GetServices<ILoggerProvider>(), logger => logger is OpenTelemetryLoggerProvider);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void AddObservability_InvalidEndpoint_FailsAtStartup(string endpoint)
    {
        using var provider = CreateProvider(endpoint);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("1.5")]
    public void AddObservability_RatioOutOfRange_FailsAtStartup(string ratio)
    {
        using var provider = CreateProvider("http://otel-lgtm:4317", ratio);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    public void AddObservability_SamplingRatio_DecidesWhichTracesAreKept(string ratio, bool kept)
    {
        using var provider = CreateProvider("http://otel-lgtm:4317", ratio);
        _ = provider.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource(_aspNetCoreSource);

        using var activity = source.StartActivity("GET /");

        Assert.Equal(kept, activity?.Recorded == true);
    }

    private static ServiceProvider CreateProvider(string? endpoint, string ratio = "1")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTelemetry:OtlpEndpoint"] = endpoint,
                ["OpenTelemetry:TraceSamplingRatio"] = ratio,
            })
            .Build();

        return new ServiceCollection().AddLogging().AddObservability(configuration).BuildServiceProvider();
    }
}
