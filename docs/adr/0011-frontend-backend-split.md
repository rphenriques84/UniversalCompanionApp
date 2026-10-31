# ADR 0011: Public frontend (BFF) and private backend API

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

We want a clear distinction between frontend and backend.
Users should authenticate to the frontend. The backend should only accept trusted service calls.
The WebAssembly client runs in the user's browser, so it can never hold service credentials.

## Decision

- **Frontend (public):** ASP.NET Core Blazor Web App. Static SSR, WebAssembly client, BFF endpoints, OAuth handshakes, session cookie. **No database, no SMTP.**
- **Backend API (private):** ASP.NET Core Minimal APIs. Game data, progress, user accounts (Identity Core), business rules, PostgreSQL, email sending, seeder.
- **Access:**
  - Browser → frontend: user account (cookie) or anonymous guest.
  - Frontend → backend: JWT service token signed by the frontend (`aud` = backend, `exp` ≤ 5 min, scopes, optional `uid` for the acting user).
- The backend is not reachable from outside (private network) and denies calls without a valid token.
- All outbound integrations with credentials (database, SMTP) are in the backend only.
- Frontend and backend share only `Domain` and `Contracts`. No project references between them.

## Consequences

- Good: backend not exposed; clear separation; independent deploy and scale; future clients (e.g. a Discord bot) can get their own service accounts.
- Bad: two containers; extra network hop; service authentication and BFF forwarding to build; cold starts can stack when scaling to zero.
- Neutral: on Azure, managed identity can replace self-signed service tokens later.

## Alternatives considered

- Single host (pages + API in one service): simpler and cheaper, but no separation.
- Split code, single deployment: separation in code only; backend still exposed in-process.
- WebAssembly client calling the backend directly: impossible to keep service credentials secret.
