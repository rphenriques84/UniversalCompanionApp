# ADR 0010: Defer hosting decision to Phase 6

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

Costs depend on real usage. Everything runs locally until Phase 6.

## Decision

Develop locally with Docker. Choose app and database hosting in Phase 6, preferring free tiers and Azure where cost-effective. See docs/11-hosting-options.md.

## Consequences

- Good: decisions based on real data; Docker keeps options open.
- Bad: deployment issues found later.

## Alternatives considered

- Decide now: premature.
