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

    private static ServiceProvider CreateProvider(string? endpoint)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenTelemetry:OtlpEndpoint"] = endpoint })
            .Build();

        return new ServiceCollection().AddLogging().AddObservability(configuration).BuildServiceProvider();
    }
}
