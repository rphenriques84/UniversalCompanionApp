using System.ComponentModel.DataAnnotations;

namespace Companion.Frontend.Features.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "OpenTelemetry";

    /// <summary>The Collector's OTLP endpoint. Never Grafana directly. Empty means no telemetry is sent.</summary>
    [Url]
    public string? OtlpEndpoint { get; set; }

    /// <summary>Share of new traces to keep, from 0 to 1. A call inside a kept trace is always kept.</summary>
    [Range(0.0, 1.0)]
    public double TraceSamplingRatio { get; set; } = 1.0;
}
