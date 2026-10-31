# ADR 0006: Game data as versioned JSON in git

- **Status:** Accepted, amended by [ADR 0013](0013-game-data-in-memory.md)
- **Date:** 2026-10-30

## Context

Game data never changes at runtime. It must be reviewable and validated, and user progress depends on stable identifiers.

## Decision

Store game data as JSON in `data/games/<game>/`, validated by JSON Schema in CI. Keys are permanent. Originally seeded into PostgreSQL by key; ADR 0013 serves it from memory instead.

## Consequences

- Good: history, reviews, CI validation, no runtime dependency.
- Bad: data entry is manual work.

## Alternatives considered

- Admin UI editing the DB: no history, more code.
- External API at runtime: none exists for FFX.
