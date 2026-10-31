# ADR 0007: Guest-first with browser storage and export file

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

Many players don't want an account. Progress must never be lost.

## Decision

Guest is the default. Progress is stored in the browser. Users can export/import a versioned JSON file. Accounts come in Phase 5.

## Consequences

- Good: zero friction, no server state for guests, user owns data.
- Bad: clearing browser data loses unsaved progress (mitigated by export reminders).

## Alternatives considered

- Account required: friction.
- Anonymous server storage: privacy and cost questions.
