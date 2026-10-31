# Logging and observability

- OpenTelemetry for logs, traces and metrics. Both services export OTLP to the **Collector** (`OpenTelemetry:OtlpEndpoint`). Never directly to Grafana.
- Only the Collector holds Grafana credentials.
- Browser errors: WebAssembly client → frontend `POST /bff/client-errors` (rate-limited, size-limited, no personal data) → frontend logger.
- Structured logs with message templates: `logger.LogInformation("Imported {Count} items", count)`. Never interpolation.
- Prefer `LoggerMessage` source-generated methods for hot paths.
- Levels: `Error` = needs action; `Warning` = unexpected but handled; `Information` = key business events; `Debug` = local only.
- Production: `Information` minimum, framework categories at `Warning`, sampled traces.
- Keep volume low (free-tier limits). Don't log per-item checkbox changes.
- Never log personal data or request/response bodies.
