# ADR 0001: .NET 10 (LTS) as runtime

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

We need a stable runtime for a long-lived side project. .NET 11 (STS) ships in November 2026.

## Decision

Use .NET 10 LTS. Move to the next LTS (.NET 12) when it ships.

## Consequences

- Good: support until November 2028, fewer forced upgrades.
- Bad: newest features arrive later.

## Alternatives considered

- .NET 11 STS: shorter support window.
