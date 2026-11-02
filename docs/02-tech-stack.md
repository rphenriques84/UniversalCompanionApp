# 02 – Tech stack

Each choice lists **why**, the **alternatives** considered and the **trade-offs**.
Decisions are recorded as ADRs in [adr/](adr/).

## Summary

| Layer | Choice | ADR |
|---|---|---|
| Runtime | .NET 10 (LTS) | [0001](adr/0001-dotnet-10-lts.md) |
| Service split | Public frontend (BFF) + private backend API | [0011](adr/0011-frontend-backend-split.md) |
| Frontend UI | Blazor Web App: static SSR + Interactive WebAssembly | [0002](adr/0002-blazor-web-app.md) |
| Styling | Tailwind CSS with CSS-variable themes | [0003](adr/0003-tailwind-themes.md) |
| Database | PostgreSQL + EF Core (Npgsql), from Phase 5 | [0004](adr/0004-postgresql.md) |
| Local environment | Docker Compose (not Aspire) | [0005](adr/0005-docker-compose.md) |
| Game data | Versioned JSON in git, served from memory | [0006](adr/0006-static-data-json.md), [0013](adr/0013-game-data-in-memory.md) |
| Guest mode and export | Browser storage + export file | [0007](adr/0007-guest-first-export.md) |
| Identity | Accounts in the backend (Identity Core); frontend does OAuth + cookie | [0008](adr/0008-identity.md) |
| Service auth | JWT service tokens, frontend → backend | [0011](adr/0011-frontend-backend-split.md) |
| Observability | OpenTelemetry → Collector (private) → Grafana | [0009](adr/0009-observability-grafana.md) |
| Hosting | Deferred | [0010](adr/0010-hosting-deferred.md) |

## Runtime: .NET 10 (LTS)

- **Why:** long-term support until November 2028. Fewer forced upgrades.
- **Alternative:** .NET 11 (Standard Term Support). Newer features, shorter support.
- **Plan:** move to the next LTS (.NET 12) when it ships.

## Service split: frontend (BFF) + backend API

- **Frontend (public):** ASP.NET Core Blazor Web App. Pages, WebAssembly client, BFF endpoints, OAuth sign-in, session cookie. No database, no SMTP.
- **Backend API (private):** ASP.NET Core Minimal APIs. Game data, progress, user accounts, business rules, database, email.
- **Why:**
  - The backend is never exposed to the internet.
  - Clear separation of frontend and backend code.
  - Independent deployment and scaling.
  - Other clients (e.g. a Discord bot) can use the backend later with their own service accounts.
- **Access model:** users authenticate to the frontend (cookie). The frontend authenticates to the backend as a service (JWT), passing the acting user ID.
- **Trade-offs:** two containers, one extra network hop, service authentication to build.
- **Important:** the WebAssembly client runs in the browser, so it never holds service credentials and never calls the backend directly.
- Details: [03-architecture.md](03-architecture.md).

## Frontend UI: Blazor Web App

- **Why:** one language (C#) across front end and back end. Shared models and validation.
- **Render modes:**

| Mode | Used for |
|---|---|
| Static SSR | Public pages: game overview, item lists. Fast, indexable. |
| Interactive WebAssembly | Checklist. Works offline, progress in the browser. Lets the server scale to zero. |

- **Not used:** Interactive Server (needs a live connection; bad on flaky phone networks and for scale-to-zero).
- **Alternative:** React/Svelte + .NET API. Better PWA/offline ecosystem, but two languages.

## Styling: Tailwind CSS

- **Why:** full control for game-specific themes. No component library look to fight.
- **How:** Tailwind tokens map to CSS variables. Each game has a theme file in `Styles/themes/`.
  Switch with `<html data-theme="ffx">`.
- **Build:** Tailwind standalone CLI via an MSBuild target. No Node toolchain required.
- **Alternative rejected:** MudBlazor (fast to start, but Material look is hard to theme deeply).
- **Trade-off:** we build our own components (checkbox, counter, progress bar, tabs, dialog).

## Backend API: Minimal APIs

- **Why:** small, fast, typed results, OpenAPI support.
- **Used for:** game data, progress, import/sync.
- Grouped per feature (`MapGroup`).
- Protected by JWT bearer service tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`).
- Frontend BFF endpoints are also Minimal APIs, protected by the user cookie and antiforgery.

## Database: PostgreSQL + EF Core (Phase 5)

- **Why:** free, host-neutral, great EF Core support, JSONB for game-specific fields.
- **Local:** Docker container. **Only the backend connects to it.**
- One database `companion`, schemas `app` (progress) and `identity`. Game data is not stored here.
- **Production:** decided later (see [11-hosting-options.md](11-hosting-options.md)).
- **Conventions:** snake_case names (`EFCore.NamingConventions`), UTC `DateTimeOffset`.
- **Alternative:** SQL Server / Azure SQL (free offer on Azure, but heavier and less host-neutral).

## Local environment: Docker Compose

- **Why:** one `docker compose up` starts everything. Same image deploys anywhere.
- **Services:** `frontend`, `backend` (internal network only), `otel-lgtm` (Grafana). `postgres` and `mailpit` are added in Phase 5.
- **Alternative rejected:** Aspire for orchestration. Good tool, but Docker Compose is simpler and host-neutral.

## Game data: versioned JSON

- **Why:** data changes only when curated. Git history, PR reviews, CI validation.
- **Load:** the backend reads validated JSON into memory at startup ([ADR 0013](adr/0013-game-data-in-memory.md)).

## Identity (Phase 5)

- ASP.NET Core Identity Core in the **backend** (accounts, hashing, lockout, tokens, email). The **frontend** runs OAuth handshakes and issues the session cookie.
- External: Google, Discord, Steam, Microsoft.
- Email/password: only for non-external sign-up. Email is the login name.
- Details: [08-identity-and-accounts.md](08-identity-and-accounts.md).

## Observability: OpenTelemetry → Grafana

- **Both services send OTLP to an OpenTelemetry Collector on the private network.** Only the Collector talks to Grafana and holds its credentials.
- **Local:** `grafana/otel-lgtm` container (Collector, Grafana, Loki, Tempo, Prometheus).
- **Production (later):** Collector container on the private network (e.g. Grafana Alloy) → Grafana Cloud Free.
- **Why a Collector (not a backend log endpoint):** telemetry still arrives when the backend is down; standard batching, retries, sampling and PII filtering; no telemetry load on the backend; no Grafana key in the public frontend.
- **Browser errors:** rate-limited frontend BFF endpoint → frontend OpenTelemetry pipeline.
- **Why the same tool in both:** same query languages (LogQL, TraceQL, PromQL), dashboards and alerts can be exported/imported.
- **Errors (optional):** Sentry free plan.
- Details: [10-observability.md](10-observability.md).

## Testing

| Tool | Scope |
|---|---|
| xUnit | Domain logic: completion %, missables, prerequisites, import/migration; service-token validation |
| bUnit | Blazor components without a browser |
| Testcontainers | Integration tests on real PostgreSQL (Phase 5) |
| Playwright for .NET | 3–5 end-to-end smoke flows |

## CI/CD: GitHub Actions

Build → format check → tests → seed validation → vulnerability check → Docker image builds (frontend, backend) → secret scan (gitleaks).
Deploy steps are added in Phase 6.
