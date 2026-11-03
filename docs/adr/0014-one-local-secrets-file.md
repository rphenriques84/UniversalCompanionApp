# ADR 0014: One local secrets file

- **Status:** Accepted
- **Date:** 2026-11-02

## Context

Local secrets lived in two places: `.env` for Docker and .NET user secrets for F5 debugging. Each place had its own keys, and user secrets sit outside the repo folder. Production will use a remote key vault later (P6.2.3), so the local choice must not leak into feature code.

## Decision

The git-ignored `.env` is the only local file with secrets, for both Docker and F5. Services read secrets only through .NET configuration. In Development, each service loads just its own keys from `.env` as the lowest-priority configuration source.

## Consequences

- Good: one place to look, and one key pair for Docker and F5.
- Good: a vault replaces the file by adding a configuration source. Feature code doesn't change.
- Good: each service still sees only its own keys, on F5 too.
- Bad: the small loader exists twice, because the frontend and backend share no project that can hold it.
- Neutral: the loader runs only in Development. Docker gets secrets from Compose as environment variables.

## Alternatives considered

- Keep user secrets: two places, and keys that differ between Docker and F5.
- A `.env` package: a new dependency, and it would load every secret into both services.
- Use a local vault emulator now: a heavy extra service, not needed before Phase 6.
