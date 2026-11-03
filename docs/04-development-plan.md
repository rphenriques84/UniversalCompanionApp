# 04 – Development plan

Guest-first. Accounts come late. All development is local (Docker) until Phase 6.
Estimates assume part-time work and are rough.

**How to read this plan**

- Each phase has **tasks** (`P0.1`) and **subtasks** (`P0.1.1`). Use the IDs in branch names, commits and PRs (e.g. `feat/p0-4-service-auth`).
- A task is done when all its subtasks are done and the [Definition of Done](05-guidelines.md#12-definition-of-done) is met.
- Each phase ends with **Done when** criteria.
- **One subtask, one PR, each a stable release.** Split a subtask here before starting it if it will not fit the PR size limit (see [05-guidelines.md](05-guidelines.md#1-git-and-workflow)). Later phases get split when they start.

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
- [x] P0.4.1 Key pair generation script (ES256). Keys stored in user-secrets / `.env`.
- [x] P0.4.2 Frontend: token issuer (`iss`, `aud`, `exp` ≤ 5 min, `scope`, optional `uid`), cached until near expiry.
- [x] P0.4.3 Frontend: `DelegatingHandler` adding the token to backend calls.
- [x] P0.4.4 Backend: JWT bearer validation (issuer, audience, lifetime, key), current + previous key.
- [x] P0.4.5 Backend: deny-by-default authorization; scope policies.
- [x] P0.4.6 Tests: missing, expired, wrong-audience and wrong-scope tokens are rejected.
- P0.4.7 Moved to P0.5.7. No service calls exist until P0.5.

### P0.5 Frontend ↔ backend communication
- [x] P0.5.0 Backend: global exception handler returning ProblemDetails. Test that no internal details leak.
- [x] P0.5.1 Backend: game DTO in `Contracts`, and `GET /games` with scope `game-data.read` returning a fixed list.
- [x] P0.5.2 Frontend: typed `HttpClient` for the backend, using the service token handler.
- [x] P0.5.3 Frontend: resilience on the typed client (timeouts, retries).
- [x] P0.5.4 BFF: `GET /bff/games` forwarding to the backend.
- [x] P0.5.5 BFF: error mapping (ProblemDetails).
- [x] P0.5.6 Prove `GET /bff/games` works end to end in Docker.
- [x] P0.5.7 Diagram: add the JWT service token link, BFF endpoints and the first backend endpoint.

### P0.6 Seed data
- P0.6.1 Moved to P5.0.1 (see [ADR 0013](adr/0013-game-data-in-memory.md)).
- P0.6.2 Moved to P5.0.2.
- P0.6.3 Replaced by P5.0.3. Game data gets no tables.
- [x] P0.6.4 JSON Schema for games in `data/schema/`.
- P0.6.5 Moved to P1.7.6.
- [x] P0.6.6 `SeedBuilder` project with a `validate` command: schema checks.
- [x] P0.6.7 `SeedBuilder validate`: each game folder has a `game.json` whose key matches the folder, which also makes keys unique.
- [x] P0.6.8 Add seed validation to CI.
- [x] P0.6.9 Minimal sample seed data to prove the flow.
- [x] P0.6.10 Copy `data/` into the backend image.
- [x] P0.6.11 Backend loader: read validated JSON from `data/` into memory on startup. Fail fast on bad data.
- [x] P0.6.12 Backend: `GET /games` serves loaded data instead of the fixed list.
- [x] P0.6.13 Diagram: add seed data, the game data loader and the load arrow.

### P0.7 Frontend base
- [x] P0.7.1 Tailwind standalone CLI via MSBuild target, working in local and Docker builds.
- [x] P0.7.2 Theme tokens as CSS variables; replace template colors.
- [x] P0.7.3 `data-theme` switching; neutral default theme.
- [x] P0.7.4 Base layout (mobile-first).
- [x] P0.7.9 Direct debugging: F5 starts the frontend and backend together with Docker off, using user secrets for throwaway local keys. Our services always run this way; infrastructure may stay in Docker (P5.0.7, P5.3.1). Document the setup.
- [x] P0.7.10 Frontend endpoint page: OpenAPI and a Scalar page in Development only, to call the `/bff/*` endpoints from the browser. Production exposes neither (tested).
- [x] P0.7.11 Backend endpoint page: OpenAPI and a Scalar page in Development only, to call the backend directly during F5 debugging. A dev tool reads the throwaway key from user secrets and issues a short-lived service token to paste in. Real token checks stay on, with no bypass. Production exposes neither (tested).
- [x] P0.7.12 One local secrets file: the git-ignored `.env` holds every local secret, for both Docker and F5, and user secrets go away. Services read secrets only through .NET configuration, so a remote key vault can replace the file later (P6.2.3) with no feature code changes. Update the tools, `docs/` and the rules. Add an ADR.
- [x] P0.7.13 Tests for the debug tool: `dev-service-token`, including a token the backend accepts.
- [x] P0.7.14 Startup and log leak tests:
  - Process tests start the compiled backend and frontend as real processes with a bad key, and the backend with a bad `GameData:Directory`. Each must exit with an error. The test reads all stdout and stderr, at `Debug` level, and fails if it finds the test key value.
  - Log capture tests use known test secrets (private key, public key, service token) in a normal run with successful and rejected calls. The test captures every log record with `FakeLogCollector` and fails if it finds a secret, in the message or in structured values.
  - Exception messages stay covered: rejected tokens must not put the token or key in logged exceptions.
- [x] P0.7.5 Resource files for UI strings.
- [x] P0.7.6 Render modes: static SSR default, WebAssembly enabled for interactive pages.
- [ ] P0.7.7 Prove the WebAssembly client runs: one interactive component in a real browser, in Docker.
- [ ] P0.7.8 Diagram: remove "no interactive pages yet", add Tailwind CSS.

### P0.8 Observability
- [ ] P0.8.1 Frontend: OpenTelemetry traces → Collector in `otel-lgtm`.
- [ ] P0.8.2 Backend: OpenTelemetry traces → Collector in `otel-lgtm`.
- [ ] P0.8.3 Logs and metrics in both services, including Error logs for unhandled exceptions.
- [ ] P0.8.8 Scan test log output for key values, not just known failure paths.
- [ ] P0.8.4 Trace propagation frontend → backend.
- [ ] P0.8.5 Low-volume defaults (levels, framework categories at `Warning`).
- [ ] P0.8.6 Verify a single trace in Grafana for `GET /bff/games`.
- [ ] P0.8.7 Diagram: add OTLP arrows from frontend and backend.
- [ ] P0.8.9 Tag logs, traces and metrics with the stack they came from (dev or test) through `deployment.environment`.

### P0.9 CI and docs
- [x] P0.9.1 GitHub Actions: build, `dotnet format --verify-no-changes`, tests, seed validation, vulnerable packages, Docker image builds.
- P0.9.2 Moved to P5.0.4.
- [ ] P0.9.3 README quickstart tested on a clean machine, including `cp .env.example .env`.
- [x] P0.9.4 Create `.github/dependabot.yml` if it doesn't exist. Add GitHub Actions (same settings).
- [x] P0.9.5 Secret scanning (gitleaks) in CI.
- [ ] P0.9.6 Confirm Dependabot opens a Docker or Docker Compose PR.
- [ ] P0.9.7 Watch CI for the 2 unexplained build errors seen once after merging P0.3. Capture the message if they return.
- [ ] P0.9.10 Test the real resilience settings: 5 s per attempt, 15 s total, and the circuit breaker opening.
- [ ] P0.9.11 Test that the real frontend refuses to start with a bad key, without printing it.
- [ ] P0.9.12 Confirm the key generator test fails when the "keys already exist" check is removed.
- [ ] P0.9.13 Automate Compose network checks: backend unreachable from the host and with no internet.
- [ ] P0.9.14 Find out why the backend exits with code 139 on a bad key, not a clean failure code.
- [ ] P0.9.15 Extend the one-type-per-file rule to `.razor` files, or record why not.
- [ ] P0.9.16 One Compose stack per test run, shared by every test project that makes real requests. A shared test helper creates it if missing, starts it if stopped, and reuses it if running, once across test processes. It stops the stack after the last project finishes and never removes it. Containers are recreated only when the code changes, so the throwaway keys stay the same between runs. Tests reach the backend only through the frontend, as in production. Tests that need their own settings or fake services stay in-process. Add this to the testing rule.
- [ ] P0.9.17 Remove `COMPANION_E2E`: E2E tests always run and fail with a clear message if Docker is off or a test port is in use. Update the testing rule and `CLAUDE.md`, and keep CI from running them twice.
- [ ] P0.9.18 Make Compose's published ports configurable, with the dev defaults unchanged (8080, 3000). The test stack uses 8081 and 3001, so both stacks can run together.
- [x] P0.9.8 Key generator writes `.env` itself as UTF-8. Windows PowerShell quickstart check in CI.
- [x] P0.9.9 Test that apps never print keys. Security rule: run Docker Compose with a temporary `--env-file`, never the real `.env`.

**Done when:** `docker compose up` shows a page that lists sample games from the backend, the backend is unreachable from the host, a trace is visible in Grafana, and CI is green.

---

## Phase 1 – Guest MVP (≈2–3 weeks)

### P1.7 FFX data (first set)
- [ ] P1.7.6 Decide the seed structure from real FFX data: JSON Schemas, including categories and items, `SeedBuilder validate` checks, and any Domain rules, endpoints and tasks it needs.
- [ ] P1.7.2 Al Bhed Primers.
- [ ] P1.7.3 Jecht Spheres.
- [ ] P1.7.4 Celestial Weapons (weapon, crest, sigil, prerequisites).
- [ ] P1.7.5 Verify every entry; own wording; attribution.

### P1.1 Game data API
- [ ] P1.1.1 `GET /games/{key}`: game detail (contract and backend endpoint, scope `game-data.read`).
- [ ] P1.1.3 Categories endpoint (contract and backend).
- [ ] P1.1.4 Items endpoint (contract and backend).
- [ ] P1.1.5 BFF endpoints for the WebAssembly client.
- [ ] P1.1.6 Output caching of public game data in the frontend.
- [ ] P1.1.7 Diagram: update the endpoint labels.

### P1.2 Public pages (static SSR)
- [ ] P1.2.1 Game list page, with a friendly error state when data cannot load.
- [ ] P1.2.2 Game overview page (categories, totals).
- [ ] P1.2.3 Category page (items, read-only).

### P1.3 Domain logic
- [ ] P1.3.1 Completion % for one category (weights).
- [ ] P1.3.2 Counter items in completion %.
- [ ] P1.3.3 Overall completion %.
- [ ] P1.3.5 Prerequisites evaluation.

### P1.4 Interactive tracking (WebAssembly)
- [ ] P1.4.1 Checkbox component (theme tokens, accessible).
- [ ] P1.4.4 Counter component (theme tokens, accessible).
- [ ] P1.4.2 Progress state service in the client.
- [ ] P1.4.3 Live completion % using the shared Domain library.

### P1.5 Browser storage
- [ ] P1.5.1 Storage abstraction (wrapped, fails safely).
- [ ] P1.5.2 Debounced auto-save.
- [ ] P1.5.3 Load on start; handle missing or corrupt data.
- [ ] P1.5.4 Diagram: add browser storage.

### P1.6 Export / import
- [ ] P1.6.1 Export format v1 schema (see [09-export-file-format.md](09-export-file-format.md)).
- [ ] P1.6.2 Export to file download.
- [ ] P1.6.3 Import: size limit and schema validation. Define the Domain result type for expected failures here.
- [ ] P1.6.7 Import: unknown keys warning.
- [ ] P1.6.4 Replace and merge modes.
- [ ] P1.6.5 Migration pipeline skeleton (v1 → future versions) with tests.
- [ ] P1.6.6 Diagram: add local export and import files.

### P1.8 FFX theme
- [ ] P1.8.1 FFX theme tokens (colors, fonts, surfaces); game pages set `data-theme="ffx"`.
- [ ] P1.8.2 Contrast check (WCAG AA).
- [ ] P1.8.3 Footer trademark notice.

### P1.9 Browser error reporting
- [ ] P1.9.1 Frontend `POST /bff/client-errors` (size-limited, no personal data).
- [ ] P1.9.3 Rate limit on `POST /bff/client-errors`.
- [ ] P1.9.2 Client: global error handler calling it.

**Done when:** a guest can track the first FFX categories on a phone, progress survives a reload, and export → import restores it.

---

## Phase 2 – Missables and spoilers (≈1–2 weeks)

### P2.1 Playthroughs
- [ ] P2.1.1 Multiple local playthroughs (create, rename, delete, switch).
- [ ] P2.1.3 Export/import includes all playthroughs.

### P2.3 Spoiler protection
- [ ] P2.3.1 Hide spoilers the player hasn't reached yet.
- [ ] P2.3.2 "Show spoilers" opt-in per playthrough.

### P2.4 Missables
- [ ] P2.4.1 Domain: "missable soon" calculation.
- [ ] P2.4.2 Warning banner before points of no return.
- [ ] P2.4.3 "Mark as missed" state; 100% shown as impossible.
- [ ] P2.4.4 Tests for all missable rules.

**Done when:** a guest sees warnings before missing items and no spoilers they haven't reached yet.

---

## Phase 3 – Full FFX content (≈3–4 weeks, mostly data)

### P3.1 Extraction pipeline
- [ ] P3.1.1 Extract game files from your own copy (outside the repo).
- [ ] P3.1.2 Run the extraction tool (FFXDataParser for FFX).
- [ ] P3.1.3 SeedBuilder importers: parser output → seed JSON.
- [ ] P3.1.4 Validation and diff report per run.
- [ ] P3.1.5 Diagram: add external data, extraction tools and the seed builder.

### P3.2 Generated data
- [ ] P3.2.1 Treasures and key items.
- [ ] P3.2.2 Monster Arena fiends (capture counters).
- [ ] P3.2.3 Sphere Grid data.

### P3.3 Curated data
- [ ] P3.3.1 Optional aeons.
- [ ] P3.3.2 Dark Aeons and Penance.
- [ ] P3.3.3 Blitzball.
- [ ] P3.3.4 Minigames (counters).
- [ ] P3.3.5 Trophies / achievements.
- [ ] P3.3.6 Overdrives.

### P3.4 Quality
- [ ] P3.4.1 Verify missables for every item.
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
- [ ] P4.5.1 Playwright smoke tests (3–5 flows) on the shared E2E stack (P0.9.16).
- [ ] P4.5.2 bUnit coverage for core components.

**Done when:** the guest-only app is complete and could be released.

---

## Phase 5 – Accounts (≈3–4 weeks)

### P5.0 Database
- [ ] P5.0.1 Backend: EF Core + Npgsql, `postgres` service with a named volume, database `companion`, schema `app`, snake_case naming.
- [ ] P5.0.2 Runtime and migrations database users.
- [ ] P5.0.3 First migration: progress tables, keyed by item key.
- [ ] P5.0.4 Testcontainers set up for backend integration tests: one shared PostgreSQL container per test run, across test projects (see P0.9.16).
- [ ] P5.0.5 Trace propagation backend → database.
- [ ] P5.0.6 Diagram: add PostgreSQL and the data access arrow.
- [ ] P5.0.7 Debug with the database: PostgreSQL stays in Docker, on a dev-only `127.0.0.1` port that F5 can reach. In Development, the backend's debug output shows every SQL query, with parameter values hidden so no personal data is logged.

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
- [ ] P5.3.1 Add Mailpit to Docker Compose (backend network), with dev-only `127.0.0.1` ports so F5 can send to it and its web page shows each message.
- [ ] P5.3.2 Backend email sender with allowed templates only.
- [ ] P5.3.3 Templates: confirm email, reset password.
- [ ] P5.3.4 Diagram: add the email service.

### P5.4 Frontend session
- [ ] P5.4.1 Cookie authentication (HttpOnly, Secure, SameSite), antiforgery.
- [ ] P5.4.2 Data Protection keys in a Docker volume.
- [ ] P5.4.3 Periodic session validation against the security stamp.
- [ ] P5.4.4 Sign-out.
- [ ] P5.4.5 Diagram: add the user session (cookie, OAuth sign-in).
- [ ] P5.4.6 Bound the service token cache once tokens carry real user IDs.

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
- [ ] P5.7.5 Retry POSTs in the backend client once they can repeat safely (idempotency keys). Turn off `DisableForUnsafeHttpMethods` for those calls.

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
- [ ] P6.2.3 Remote key vault replaces the local secrets file (P0.7.12): service-token keys, OAuth secrets, SMTP, Grafana key (or managed identity on Azure).
- [ ] P6.2.4 Frontend Data Protection key storage.

### P6.3 Observability
- [ ] P6.3.1 Collector container (e.g. Grafana Alloy) on the private network → Grafana Cloud Free.
- [ ] P6.3.2 Collector processors: batching, sampling, PII filtering, rate limits.
- [ ] P6.3.3 Optional Sentry.
- [ ] P6.3.4 Diagram: split the Collector from Grafana and show production hosting.

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
