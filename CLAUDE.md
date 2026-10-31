# CLAUDE.md – Universal Companion App

Instructions for Claude Code in this repo. Detailed rules are in `.claude/rules/` (loaded automatically).
Human-readable versions are in `docs/` (start at `docs/README.md`).

## Project

- Web app for **100% completion tracking** in games. First game: **Final Fantasy X HD**.
- **Guest-first:** progress lives in the browser; export/import file. Accounts come in Phase 5.
- Current phase: **Phase 0** (see `docs/04-development-plan.md`).

## Stack

- **Two services:**
  - **Frontend (public):** Blazor Web App (static SSR + Interactive WebAssembly; **no Interactive Server**), BFF endpoints, OAuth sign-in and session cookie (Phase 5). **No database, no SMTP.**
  - **Backend API (private):** Minimal APIs, game data (JSON in memory), progress, user accounts (Identity Core), email. PostgreSQL from Phase 5. Accepts only JWT service tokens from the frontend.
- .NET 10 LTS.
- Tailwind CSS with per-game themes via CSS variables (`data-theme`). No component libraries.
- Game data: JSON from `data/`, loaded into memory by the backend (ADR 0013).
- Phase 5: EF Core + PostgreSQL (Npgsql, snake_case). Database `companion`, schemas `app` and `identity`, for progress and accounts. Backend only.
- Docker Compose locally (`frontend`, `backend` on internal network only, `otel-lgtm`; `postgres` and `mailpit` from Phase 5). **Not Aspire.**
- OpenTelemetry → Collector on the private network → Grafana (`otel-lgtm` locally, Collector + Grafana Cloud later). Only the Collector talks to Grafana.
- Tests: xUnit, bUnit, Playwright; Testcontainers from Phase 5.

## Commands

```bash
cp .env.example .env            # first time only
dotnet tools/generate-service-keys.cs >> .env   # first time only
docker compose up -d            # start everything
dotnet build                    # build
dotnet test                     # all tests (Docker needed for Testcontainers from Phase 5)
dotnet format --verify-no-changes
dotnet run --project tools/Companion.SeedBuilder -- validate data/   # seed validation (from P0.6.6)
```

## Must always

- Follow `.claude/rules/*`. If a request conflicts with them, say so before acting.
- Keep `Domain` and `Contracts` free of EF Core and ASP.NET references.
- Keep frontend and backend separate: no project references between them; they talk over HTTP only.
- Route all WebAssembly client calls through frontend BFF endpoints. Protect every backend endpoint with service-token auth.
- Take the user ID in the backend only from the service token's `uid` claim.
- Treat item keys in `data/` as **permanent**: never rename, reuse or delete. Use `"deprecated": true`.
- Write tests with changes to domain logic, import/export or migrations.
- Use structured logging with message templates.
- Use `TimeProvider` and UTC `DateTimeOffset`.
- Propose an ADR (`docs/adr/`) for hard-to-reverse decisions.
- Update docs when behavior or decisions change.

## Must never

- Commit secrets, `.env` files, service-token keys, game files or extracted game assets.
- Call the backend from the WebAssembly client, or put any secret in client code.
- Publish the backend's port in production configuration.
- Give the frontend database or SMTP access.
- Send telemetry directly to Grafana from any service, or from the WebAssembly client to anything but the frontend BFF.
- Copy text from commercial guides (Game8, Jegged, GameFAQs, Prima). Write notes in our own words.
- Log personal data (emails, tokens, IPs, OAuth payloads, bodies).
- Use `.Result` / `.Wait()`, `DateTime.Now`, or string interpolation in log calls.
- Hard-code colors in components (use theme tokens).
- Add packages without Central Package Management (`Directory.Packages.props`).
- Use `:latest` Docker tags.

## Style of answers

The owner prefers concise, short, easy sentences. Offer detail on request.
