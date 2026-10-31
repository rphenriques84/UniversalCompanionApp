# ADR 0013: Serve game data from JSON, postpone PostgreSQL to Phase 5

- **Status:** Accepted
- **Date:** 2026-10-31
- **Amends:** ADR 0004, ADR 0006

## Context

ADR 0006 seeded game data into PostgreSQL. Before Phase 5 that would be the database's only content.
Game data is read-only and changes only through Git, so a database would keep a second copy of it.
Accounts and synced progress do need a database, but they arrive in Phase 5.

## Decision

The backend loads validated JSON from `data/` into memory at startup and serves game data from there.
PostgreSQL is added in Phase 5 for accounts and progress only. Progress references items by their permanent key.

## Review process

Review this decision at two checkpoints, and earlier if a trigger below appears:

- **P3.4.4:** after the full FFX data set exists, using the numbers from P3.4.3.
- **P5.1.3:** before PostgreSQL is added, when moving would cost least.

Triggers (starting values, adjust them with real data):

| Trigger | Threshold |
|---|---|
| Backend memory used by game data | Over 100 MB |
| Added backend startup time | Over 5 seconds |
| Slow queries | Over 200 ms after in-memory indexing |
| Queries | Needed search or filters are hard to do in C# |
| Live changes | Game data must change without a deploy |
| Data integrity | Broken item keys in progress data occur in practice |

Move game data to PostgreSQL only if a trigger is met and a simple fix, such as an in-memory index, doesn't solve it. Record the result in a new ADR.

## Consequences

- Good: one source of truth; no seeder, migrations or database for a guest-only release; lower hosting cost.
- Bad: no foreign keys from progress to items; every backend copy holds all game data in memory; a data change needs a deploy.
- Neutral: Testcontainers, database users and migrations move to Phase 5.

## Alternatives considered

- Keep PostgreSQL from Phase 0: two copies of the data, and hosting cost with no current benefit.
- Client reads JSON directly: bypasses the backend and its contracts.
