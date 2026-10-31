# ADR 0005: Docker Compose for local development

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

We need a reproducible local environment with the app, database and observability.

## Decision

Use Docker Compose with services `frontend`, `backend` (internal network only, no published port), `postgres`, `otel-lgtm` (and `mailpit` from Phase 5). Pinned image versions.

## Consequences

- Good: one command, same image deploys anywhere.
- Bad: no Aspire service discovery or dashboard (Grafana covers telemetry).

## Alternatives considered

- .NET Aspire: good, but adds a layer we don't need.
