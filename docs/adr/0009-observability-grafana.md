# ADR 0009: OpenTelemetry with Grafana locally and in production

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

We want useful logs, traces and metrics at minimal cost, with the option to switch vendors.

## Decision

Instrument with OpenTelemetry. Both services send OTLP to an OpenTelemetry Collector on the private network; only the Collector talks to Grafana and holds its credentials. Locally the Collector and Grafana are the `grafana/otel-lgtm` container. In production: a Collector container (e.g. Grafana Alloy) → Grafana Cloud Free. Browser errors go through a rate-limited frontend BFF endpoint. Keep log volume low.

## Consequences

- Good: same tool and query languages everywhere, free tiers, vendor-neutral.
- Bad: local container is heavier than Aspire Dashboard.

## Alternatives considered

- Aspire Dashboard: light, but dev-only and no production counterpart.
- Datadog: no free logs.
- Backend endpoint relaying frontend logs: telemetry lost when the backend is down, re-implements OTLP, adds load to the backend.
- Services exporting directly to Grafana Cloud: puts a Grafana key in the public frontend.
- Elastic: no permanent free cloud tier; self-hosting is heavy.
