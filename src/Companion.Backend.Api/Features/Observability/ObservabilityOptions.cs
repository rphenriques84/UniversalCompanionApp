using System.ComponentModel.DataAnnotations;

namespace Companion.Backend.Api.Features.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "OpenTelemetry";

    /// <summary>The Collector's OTLP endpoint. Never Grafana directly. Empty means no telemetry is sent.</summary>
    [Url]
    public string? OtlpEndpoint { get; set; }
}
