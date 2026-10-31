# ADR 0004: PostgreSQL as database

- **Status:** Accepted, amended by [ADR 0013](0013-game-data-in-memory.md)
- **Date:** 2026-10-30

## Context

We want the lowest cost and host neutrality. Local development runs in Docker.

## Decision

Use PostgreSQL with EF Core (Npgsql) and snake_case naming. One database `companion` with schemas `app` and `identity`, used **only by the backend**. Run it in a local Docker container. Production host decided later (ADR 0010).

## Consequences

- Good: free, many free/cheap hosts (Neon, Azure Flexible Server free year, self-hosted), JSONB.
- Bad: Azure SQL free offer would be cheaper long-term on Azure specifically.

## Alternatives considered

- SQL Server / Azure SQL: heavier image, less host-neutral.
- SQLite: weak for concurrent writes.
