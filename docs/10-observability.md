# 10 – Observability

## Approach

- Instrument **once** with OpenTelemetry (logs, traces, metrics) using standard `ILogger`, `ActivitySource` and `Meter`.
- The destination is configuration only (OTLP endpoint + headers).
- Both services (`companion-frontend`, `companion-backend`) export telemetry. W3C trace context propagates, so one trace covers browser request → frontend → backend → database.
- Same tool in every environment: **Grafana**.

```
frontend ─┐
          ├─ OTLP ─► OpenTelemetry Collector (private network) ─► Grafana
backend ──┘
browser (WebAssembly) ─► frontend /bff/client-errors ─► frontend logger ─► Collector
```

| Environment | Collector | Grafana | Cost |
|---|---|---|---|
| Local | Built into `grafana/otel-lgtm` | Same container (UI on `127.0.0.1:3000`) | Free |
| Production (later) | Collector container on the private network (e.g. Grafana Alloy) | Grafana Cloud Free | Free tier + one small container |
| Errors (optional) | — | Sentry free plan | Free within limits |

### Why a Collector

- **Only the Collector holds Grafana credentials** and has outbound telemetry access. No Grafana key in the public frontend.
- Telemetry still arrives when the backend is down (unlike relaying logs through a backend endpoint).
- Built-in batching, retries, sampling, rate limits and **PII filtering** before data leaves our network.
- No telemetry load on the backend API.

### Browser errors

- The WebAssembly client never sends telemetry to the Collector or Grafana.
- Unhandled client errors → `POST /bff/client-errors` on the frontend:
  - anonymous allowed, rate-limited per IP/session, max body size (e.g. 8 KB),
  - message, stack trace, page, app version; **no personal data**,
  - logged by the frontend at `Warning`/`Error` through its normal OpenTelemetry pipeline.

### Why Grafana locally instead of Aspire Dashboard

- Same query languages as production: LogQL (logs), TraceQL (traces), PromQL (metrics).
- Dashboards and alerts built locally can be exported to Grafana Cloud.
- Trade-off: heavier container (roughly 1 GB RAM) and data is lost on restart unless a volume is added.

### Grafana Cloud Free limits (checked October 2026)

- 10k active metric series, 50 GB logs, 50 GB traces, 50 GB profiles per month.
- 14-day retention, 3 users.
- Re-check before launch; free tiers change.

### Other platforms considered

| Platform | Free tier | Why not now |
|---|---|---|
| Datadog | 5 hosts infra, 1-day metrics, **no logs** | Logs cost per GB + per indexed event |
| Elastic | 14-day cloud trial; self-managed is free | Self-hosting needs several GB RAM |
| New Relic | 100 GB/month, 1 full user | Viable alternative |

OpenTelemetry keeps all of these one config change away.

## Configuration (planned)

```json
"OpenTelemetry": {
  "ServiceName": "companion-frontend",   // "companion-backend" in the backend
  "OtlpEndpoint": "http://otel-lgtm:4317",   // the Collector; never Grafana directly
  "TraceSamplingRatio": 1.0
}
```

Production: `TraceSamplingRatio` around `0.1`. Services point at the internal Collector and need **no API key**; the Grafana Cloud key is configured only in the Collector.

## Log rules

- Message templates, never interpolation.
- Levels: `Error` (needs action), `Warning` (handled but unexpected), `Information` (key business events), `Debug` (local only).
- Production minimum: `Information`. Framework noise (`Microsoft.*`, `System.*`) at `Warning`.
- Never log: emails, tokens, passwords, IPs, OAuth payloads, request/response bodies.

## Key signals

| Signal | Type |
|---|---|
| Request rate, latency, error rate (per service) | Metrics (built-in ASP.NET Core) |
| Frontend → backend calls (latency, failures, token errors) | Traces + metrics (`HttpClient` instrumentation) |
| `companion.export.created`, `companion.import.completed` (count, migrated version) | Metrics + Information logs |
| Game data load (items loaded, duration) | Information log |
| Emails sent / failed (template, no address) | Metrics + Information/Error logs (backend) |
| Unhandled exceptions | Error logs (+ Sentry if enabled) |

## Budget

- Target: < 25% of free-tier limits.
- Alert at 50% of the monthly log/trace quota.
