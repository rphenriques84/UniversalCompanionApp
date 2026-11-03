using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Companion.Backend.Api.Features.Observability;

public static class ObservabilityExtensions
{
    public const string ServiceName = "companion-backend";

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
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation())
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation())
            .WithLogging()
            .UseOtlpExporter(OtlpExportProtocol.Grpc, endpointUri);

        return services;
    }
}
