using Companion.Frontend.Features.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Companion.Frontend.Tests.Features.Observability;

public sealed class ObservabilityExtensionsTests
{
    [Fact]
    public void AddObservability_WithEndpoint_RegistersTracing()
    {
        using var provider = CreateProvider("http://otel-lgtm:4317");

        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddObservability_NoEndpoint_SendsNothing()
    {
        using var provider = CreateProvider(null);

        Assert.Null(provider.GetService<TracerProvider>());
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

        return new ServiceCollection().AddObservability(configuration).BuildServiceProvider();
    }
}
