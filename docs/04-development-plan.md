# 04 – Development plan

Guest-first. Accounts come late. All development is local (Docker) until Phase 6.
Estimates assume part-time work and are rough.

**How to read this plan**

- Each phase has **tasks** (`P0.1`) and **subtasks** (`P0.1.1`). Use the IDs in branch names, commits and PRs (e.g. `feat/p0-4-service-auth`).
- A task is done when all its subtasks are done and the [Definition of Done](05-guidelines.md#12-definition-of-done) is met.
- Each phase ends with **Done when** criteria.

---

## Phase 0 – Local foundations (≈1–2 weeks)

### P0.1 Repository and tooling
- [x] P0.1.1 Create the GitHub repository. Push the existing docs.
- [x] P0.1.2 Run `_setup/install.ps1` (moves `.claude/` and `.github/` files into place). Delete `_setup/`.
- P0.1.3 Moved to P6.1.3. Branch protection needs a public repo or GitHub Pro. Until then, the PR-only rule is followed by hand.
- [x] P0.1.4 Dependabot alerts and security updates on. Version update config is added per ecosystem in P0.2.7, P0.3.8 and P0.9.4.
- P0.1.5 Moved to P0.9.5. Secret scanning needs CI to run in.

### P0.2 Solution skeleton
- [x] P0.2.1 Create the solution and projects: `Domain`, `Contracts`, `Frontend`, `Frontend.Client`, `Backend.Api`, `Backend.Data`.
- [x] P0.2.2 Create test projects: `Domain.Tests`, `Backend.Tests`, `Frontend.Tests`, `Architecture.Tests`, `E2E`.
- [x] P0.2.3 `Directory.Build.props`: .NET 10, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`.
- [x] P0.2.4 `Directory.Packages.props` (Central Package Management).
- [x] P0.2.5 Set project references per the dependency rules (see [03-architecture.md](03-architecture.md#dependency-rules)).
- [x] P0.2.6 Architecture tests: Domain/Contracts have no EF/ASP.NET; frontend ↛ backend and backend ↛ frontend.
- [x] P0.2.7 Create `.github/dependabot.yml` if it doesn't exist. Add NuGet (weekly, grouped minor and patch, `chore:` prefix).

### P0.3 Docker environment
- [x] P0.3.1 Dockerfile for the frontend (multi-stage, non-root).
- [x] P0.3.2 Dockerfile for the backend (multi-stage, non-root).
- [x] P0.3.3 `docker-compose.yml`: `frontend`, `backend`, `otel-lgtm`. Pinned image versions.
- [x] P0.3.4 Networks: backend has no published port.
- [x] P0.3.5 Health checks and `depends_on: service_healthy`.
- [x] P0.3.6 `.env.example` with all settings. (PostgreSQL volume moved to P5.0.1.)
- [x] P0.3.7 Optional `docker-compose.override.example.yml` (copy to `docker-compose.override.yml`) exposing the backend on `127.0.0.1` for debugging.
- [x] P0.3.8 Create `.github/dependabot.yml` if it doesn't exist. Add Docker and Docker Compose (same settings).

### P0.4 Service authentication (frontend → backend)
- [ ] P0.4.1 Key pair generation script (ES256). Keys stored in user-secrets / `.env`.
- [ ] P0.4.2 Frontend: token issuer (`iss`, `aud`, `exp` ≤ 5 min, `scope`, optional `uid`), cached until near expiry.
- [ ] P0.4.3 Frontend: `DelegatingHandler` adding the token to backend calls.
- [ ] P0.4.4 Backend: JWT bearer validation (issuer, audience, lifetime, key), current + previous key.
- [ ] P0.4.5 Backend: deny-by-default authorization; scope policies.
- [ ] P0.4.6 Tests: missing, expired, wrong-audience and wrong-scope tokens are rejected.

### P0.5 Frontend ↔ backend communication
- [ ] P0.5.1 Typed `HttpClient` for the backend with resilience (timeouts, retries).
- [ ] P0.5.2 BFF endpoint pattern: thin forward, error mapping (ProblemDetails).
- [ ] P0.5.3 First end-to-end call: `GET /bff/games` → backend `GET /games`.

### P0.6 Seed data
- P0.6.1 Moved to P5.0.1 (see [ADR 0013](adr/0013-game-data-in-memory.md)).
- P0.6.2 Moved to P5.0.2.
- P0.6.3 Replaced by P5.0.3. Game data gets no tables.
- [ ] P0.6.4 Seed JSON Schemas in `data/schema/`.
- [ ] P0.6.5 `SeedBuilder validate` command (schema, unique keys, valid references).
- [ ] P0.6.6 Backend loader: read validated JSON from `data/` into memory on startup. Fail fast on bad data. Copy `data/` into the backend image.
- [ ] P0.6.7 Minimal sample seed data to prove the flow.

### P0.7 Frontend base
- [ ] P0.7.1 Tailwind standalone CLI via MSBuild target.
- [ ] P0.7.2 Theme tokens as CSS variables; `data-theme` switching; neutral default theme.
- [ ] P0.7.3 Base layout (mobile-first), resource files for UI strings.
- [ ] P0.7.4 Render modes: static SSR default, WebAssembly enabled for interactive pages.

### P0.8 Observability
- [ ] P0.8.1 OpenTelemetry in both services (logs, traces, metrics) → Collector in `otel-lgtm`.
- [ ] P0.8.2 Trace propagation frontend → backend.
- [ ] P0.8.3 Low-volume defaults (levels, framework categories at `Warning`).
- [ ] P0.8.4 Verify a single trace in Grafana for `GET /bff/games`.

### P0.9 CI and docs
- [ ] P0.9.1 GitHub Actions: build, `dotnet format --verify-no-changes`, tests, seed validation, vulnerable packages, Docker image builds.
- P0.9.2 Moved to P5.0.4.
- [ ] P0.9.3 README quickstart tested on a clean machine.
- [ ] P0.9.4 Create `.github/dependabot.yml` if it doesn't exist. Add GitHub Actions (same settings).
- [ ] P0.9.5 Secret scanning (gitleaks) in CI.

**Done when:** `docker compose up` shows a page that lists sample games from the backend, the backend is unreachable from the host, a trace is visible in Grafana, and CI is green.

---

## Phase 1 – Guest MVP (≈2–3 weeks)

### P1.1 Game data API
- [ ] P1.1.1 Backend endpoints: games, game detail, chapters, categories, items (scope `game-data.read`).
- [ ] P1.1.2 Contracts (DTOs) in `Companion.Contracts`.
- [ ] P1.1.3 BFF endpoints for the WebAssembly client.
- [ ] P1.1.4 Output caching of public game data in the frontend.

### P1.2 Public pages (static SSR)
- [ ] P1.2.1 Game list page.
- [ ] P1.2.2 Game overview page (categories, totals).
- [ ] P1.2.3 Category page (items, read-only).

### P1.3 Domain logic
- [ ] P1.3.1 Completion % per category and overall (weights, counters).
- [ ] P1.3.2 Edition filtering in totals.
- [ ] P1.3.3 Prerequisites evaluation.
- [ ] P1.3.4 Full unit test coverage.

### P1.4 Interactive tracking (WebAssembly)
- [ ] P1.4.1 Checkbox and counter components (theme tokens, accessible).
- [ ] P1.4.2 Progress state service in the client.
- [ ] P1.4.3 Live completion % using the shared Domain library.

### P1.5 Browser storage
- [ ] P1.5.1 Storage abstraction (wrapped, fails safely).
- [ ] P1.5.2 Debounced auto-save.
- [ ] P1.5.3 Load on start; handle missing or corrupt data.

### P1.6 Export / import
- [ ] P1.6.1 Export format v1 schema (see [09-export-file-format.md](09-export-file-format.md)).
- [ ] P1.6.2 Export to file download.
- [ ] P1.6.3 Import: size limit, schema validation, unknown keys warning.
- [ ] P1.6.4 Replace and merge modes.
- [ ] P1.6.5 Migration pipeline skeleton (v1 → future versions) with tests.

### P1.7 FFX data (first set)
- [ ] P1.7.1 Editions and chapters.
- [ ] P1.7.2 Al Bhed Primers.
- [ ] P1.7.3 Jecht Spheres.
- [ ] P1.7.4 Celestial Weapons (weapon, crest, sigil, prerequisites).
- [ ] P1.7.5 Verify every entry; own wording; attribution.

### P1.8 FFX theme
- [ ] P1.8.1 FFX theme tokens (colors, fonts, surfaces).
- [ ] P1.8.2 Contrast check (WCAG AA).
- [ ] P1.8.3 Footer trademark notice.

### P1.9 Browser error reporting
- [ ] P1.9.1 Frontend `POST /bff/client-errors` (rate-limited, size-limited, no personal data).
- [ ] P1.9.2 Client: global error handler calling it.

**Done when:** a guest can track the first FFX categories on a phone, progress survives a reload, and export → import restores it.

---

## Phase 2 – Missables and spoilers (≈1–2 weeks)

### P2.1 Playthroughs
- [ ] P2.1.1 Multiple local playthroughs (create, rename, delete, switch).
- [ ] P2.1.2 Edition per playthrough; edition filtering in views.
- [ ] P2.1.3 Export/import includes all playthroughs.

### P2.2 Current chapter
- [ ] P2.2.1 Chapter selector per playthrough.
- [ ] P2.2.2 Store current chapter in browser storage and export file.

### P2.3 Spoiler protection
- [ ] P2.3.1 Hide spoiler items past the current chapter.
- [ ] P2.3.2 "Show spoilers" opt-in per playthrough.

### P2.4 Missables
- [ ] P2.4.1 Domain: "missable soon" calculation with look-ahead.
- [ ] P2.4.2 Warning banner before points of no return.
- [ ] P2.4.3 "Mark as missed" state; 100% shown as impossible.
- [ ] P2.4.4 Tests for all missable rules.

**Done when:** a guest sees warnings before missing items and no spoilers past their chapter.

---

## Phase 3 – Full FFX content (≈3–4 weeks, mostly data)

### P3.1 Extraction pipeline
- [ ] P3.1.1 Extract game files from your own copy (outside the repo).
- [ ] P3.1.2 Run the extraction tool (FFXDataParser for FFX).
- [ ] P3.1.3 SeedBuilder importers: parser output → seed JSON.
- [ ] P3.1.4 Validation and diff report per run.

### P3.2 Generated data
- [ ] P3.2.1 Treasures and key items.
- [ ] P3.2.2 Monster Arena fiends (capture counters).
- [ ] P3.2.3 Sphere Grid data.

### P3.3 Curated data
- [ ] P3.3.1 Optional aeons.
- [ ] P3.3.2 Dark Aeons and Penance (edition-tagged).
- [ ] P3.3.3 Blitzball.
- [ ] P3.3.4 Minigames (counters).
- [ ] P3.3.5 Trophies / achievements.
- [ ] P3.3.6 Overdrives.

### P3.4 Quality
- [ ] P3.4.1 Verify missables and chapters for every item.
- [ ] P3.4.2 Own wording; attribution on every sourced item.
- [ ] P3.4.3 Performance check with the full data set (page size, WASM load).
- [ ] P3.4.4 Review game data storage against the [ADR 0013](adr/0013-game-data-in-memory.md) triggers.

**Done when:** every category in [12-ffx-completion-scope.md](12-ffx-completion-scope.md) is in the app and verified.

---

## Phase 4 – Polish (≈1–2 weeks)

### P4.1 UX
- [ ] P4.1.1 Search across items.
- [ ] P4.1.2 Filters: done/not done, missable, category, location.
- [ ] P4.1.3 Theme polish and animations (respect reduced motion).

### P4.2 Accessibility
- [ ] P4.2.1 Keyboard navigation and focus audit.
- [ ] P4.2.2 Screen reader labels.
- [ ] P4.2.3 Contrast check for all themes.

### P4.3 Security hardening
- [ ] P4.3.1 Security headers (CSP, HSTS, Referrer-Policy, X-Content-Type-Options).
- [ ] P4.3.2 Rate limiting on public BFF endpoints.

### P4.4 Observability
- [ ] P4.4.1 Grafana dashboards: errors, latency, usage per service.
- [ ] P4.4.2 Alerts: error rate, log volume at 50% of budget.

### P4.5 Testing
- [ ] P4.5.1 Playwright smoke tests (3–5 flows).
- [ ] P4.5.2 bUnit coverage for core components.

**Done when:** the guest-only app is complete and could be released.

---

## Phase 5 – Accounts (≈3–4 weeks)

### P5.0 Database
- [ ] P5.0.1 Backend: EF Core + Npgsql, `postgres` service with a named volume, database `companion`, schema `app`, snake_case naming.
- [ ] P5.0.2 Runtime and migrations database users.
- [ ] P5.0.3 First migration: progress tables, keyed by item key.
- [ ] P5.0.4 Testcontainers set up for backend integration tests.
- [ ] P5.0.5 Trace propagation backend → database.

### P5.1 Decisions
- [ ] P5.1.1 Account linking policy.
- [ ] P5.1.2 Recovery approach for external-only accounts.
- [ ] P5.1.3 Review game data storage against the [ADR 0013](adr/0013-game-data-in-memory.md) triggers.

### P5.2 Backend accounts
- [ ] P5.2.1 Identity Core (`AddIdentityCore`) with EF stores in schema `identity`.
- [ ] P5.2.2 Email uniqueness: `RequireUniqueEmail = false` + filtered unique index.
- [ ] P5.2.3 `/auth/*` endpoints (scope `auth` only), rate-limited.
- [ ] P5.2.4 Security stamp endpoint for session validation.

### P5.3 Email
- [ ] P5.3.1 Add Mailpit to Docker Compose (backend network).
- [ ] P5.3.2 Backend email sender with allowed templates only.
- [ ] P5.3.3 Templates: confirm email, reset password.

### P5.4 Frontend session
- [ ] P5.4.1 Cookie authentication (HttpOnly, Secure, SameSite), antiforgery.
- [ ] P5.4.2 Data Protection keys in a Docker volume.
- [ ] P5.4.3 Periodic session validation against the security stamp.
- [ ] P5.4.4 Sign-out.

### P5.5 External sign-in
- [ ] P5.5.1 Google.
- [ ] P5.5.2 Discord.
- [ ] P5.5.3 Steam (OpenID 2.0, no email).
- [ ] P5.5.4 Microsoft.
- [ ] P5.5.5 Backend `POST /auth/external-login` (find or create user → `uid`).

### P5.6 Email/password
- [ ] P5.6.1 Sign-up page → backend register.
- [ ] P5.6.2 Email confirmation flow.
- [ ] P5.6.3 Sign-in with lockout.
- [ ] P5.6.4 Password reset flow.
- [ ] P5.6.5 Optional TOTP 2FA.

### P5.7 Progress sync
- [ ] P5.7.1 Backend progress tables and endpoints (keyed by `uid`, scopes `progress.*`).
- [ ] P5.7.2 BFF sync endpoints (cookie + antiforgery).
- [ ] P5.7.3 Client sync: local-first, background sync, last-write-wins per item.
- [ ] P5.7.4 Guest → account migration prompt on first sign-in.

### P5.8 Privacy
- [ ] P5.8.1 Account deletion (frontend → backend; security stamp change).
- [ ] P5.8.2 Personal data export.

**Done when:** a user can sign in with each provider or email, progress syncs across devices, and accounts can be deleted.

---

## Phase 6 – Hosting and launch (≈1–2 weeks)

### P6.1 Hosting choice
- [ ] P6.1.1 Compare options with real data (see [11-hosting-options.md](11-hosting-options.md)).
- [ ] P6.1.2 Choose hosts: frontend, backend (private), Collector, PostgreSQL, email provider.
- [ ] P6.1.3 Decide repository host and CI/CD platform: GitHub public, GitHub Pro, or move to Azure DevOps. Then enforce branch protection on `main`: PRs only, CI required.

### P6.2 Infrastructure
- [ ] P6.2.1 Backend on internal-only ingress / private network.
- [ ] P6.2.2 PostgreSQL instance, users, backups.
- [ ] P6.2.3 Secret store for service-token keys, OAuth secrets, SMTP, Grafana key (or managed identity on Azure).
- [ ] P6.2.4 Frontend Data Protection key storage.

### P6.3 Observability
- [ ] P6.3.1 Collector container (e.g. Grafana Alloy) on the private network → Grafana Cloud Free.
- [ ] P6.3.2 Collector processors: batching, sampling, PII filtering, rate limits.
- [ ] P6.3.3 Optional Sentry.

### P6.4 Deployment
- [ ] P6.4.1 CI/CD deploy workflow (images, migrations, seeding).
- [ ] P6.4.2 Custom domain and TLS.
- [ ] P6.4.3 Production OAuth callback URLs.
- [ ] P6.4.4 Real email provider.

### P6.5 Launch readiness
- [ ] P6.5.1 Privacy policy and terms.
- [ ] P6.5.2 Uptime check and cost alerts.
- [ ] P6.5.3 Restore test from backup.

**Done when:** the app is public, monitored, backed up and within budget.

**Option:** run Phase 6 right after Phase 4 to launch guest-only, then add accounts. A guest-only launch needs no PostgreSQL.

---

## Phase 7 – Later

- Twitch / Epic sign-in.
- FFX-2 and other games, each with a theme.
- Save file import (PC Steam saves).
- Translations.
- Other backend clients (e.g. a Discord bot) with their own service accounts.
