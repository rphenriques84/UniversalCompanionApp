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
        var endpoint = section.Get<ObservabilityOptions>()?.OtlpEndpoint;
        if (string.IsNullOrEmpty(endpoint))
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(endpoint)));

        return services;
    }
}
