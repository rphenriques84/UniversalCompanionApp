using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Companion.Frontend.Features.Observability;

public static class ObservabilityExtensions
{
    public const string ServiceName = "companion-frontend";

    // Request metrics only. The framework's other meters (memory pool, Kestrel, DNS) add series we don't use.
    private static readonly string[] _meters = ["Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Diagnostics", "System.Net.Http"];

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(ObservabilityOptions.SectionName);
        services.AddOptions<ObservabilityOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // F5 debugging and most tests run without a Collector, so they send nothing.
        // An invalid endpoint also sends nothing here: options validation stops the app at startup.
        var endpoint = section.Get<ObservabilityOptions>()?.OtlpEndpoint;
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            return services;
        }

        // Unhandled exceptions are logged at Error by the framework's exception handler, so they reach the Collector too.
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .SetSampler(CreateSampler)
                .AddAspNetCoreInstrumentation(aspNetCore => aspNetCore.Filter = IsTraced)
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics.AddMeter(_meters))
            .WithLogging()
            .UseOtlpExporter(OtlpExportProtocol.Grpc, endpointUri);

        return services;
    }

    /// <summary>Pages and BFF calls are traced. Static files (one page load fetches dozens) are not.</summary>
    public static bool IsTraced(HttpContext context) => !Path.HasExtension(context.Request.Path.Value);

    // Parent-based: a call that arrives inside a kept trace stays in it, whatever the ratio.
    private static Sampler CreateSampler(IServiceProvider services) =>
        new ParentBasedSampler(new TraceIdRatioBasedSampler(
            services.GetRequiredService<IOptions<ObservabilityOptions>>().Value.TraceSamplingRatio));
}
