# ADR 0008: ASP.NET Core Identity with external logins and optional email/password

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

Signed-in users need sync. Gamers prefer platform logins. Some users want email/password.

## Decision

User accounts live in the backend with ASP.NET Core Identity Core (hashing, lockout, tokens, email). The frontend runs OAuth handshakes and issues the session cookie after the backend confirms the user; it has no database (ADR 0011). External: Google, Discord, Steam, Microsoft. Email/password only for non-external sign-up, with email as the login name. External accounts store no email. Implemented in Phase 5.

## Consequences

- Good: built-in, free, data stays with us.
- Bad: we own email flows (confirmation, reset) and security of password accounts.

## Alternatives considered

- Auth0 / Entra External ID: managed, but cost and complexity.
- PlayStation / Nintendo: no public web sign-in.
