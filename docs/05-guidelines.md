# 05 – Rules, good practices and guidelines

These are the project rules. The same rules are in [`/.claude/rules`](../.claude/rules) for Claude Code.
Change them through a PR (and an ADR if it's a real decision).

## 1. Git and workflow

- Trunk-based. `main` always builds and passes tests.
- Short-lived branches: `feat/...`, `fix/...`, `data/...`, `docs/...`, `chore/...`.
- Conventional Commits: `feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`, `data:`.
- Squash-merge PRs, even when solo. CI must be green.
- No direct pushes to `main`. GitHub branch protection comes in P6.1.3.
- ADR for every hard-to-reverse decision (`docs/adr/NNNN-title.md`).

## 2. .NET code standards

- `.editorconfig` enforced with `dotnet format` in CI.
- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-recommended`, `ImplicitUsings=enable`.
- Central Package Management (`Directory.Packages.props`).
- Async all the way. Pass `CancellationToken`. Never `.Result` or `.Wait()`.
- Records for DTOs and value objects. File-scoped namespaces.
- Options pattern for configuration. No magic strings.
- Use `TimeProvider`, never `DateTime.Now`. Store UTC `DateTimeOffset`.
- Private fields: `_camelCase`. Async methods end in `Async`.
- Prefer clear code over clever code. Small methods, early returns.

## 3. Architecture

- **Frontend and backend are separate services.** They share only `Domain` and `Contracts` (DTOs).
  - `Frontend.Client → Domain, Contracts`
  - `Frontend → Frontend.Client, Contracts, Domain`
  - `Backend.Api → Backend.Data → Domain`, `Backend.Api → Contracts`
  - Frontend projects never reference Backend projects, and the reverse.
- Domain and Contracts have no EF/ASP.NET references.
- Enforced by architecture tests.
- Keep the six source projects ([ADR 0012](adr/0012-project-structure.md)). Don't merge `Backend.Data` into `Backend.Api`, and don't add Application or Infrastructure layers.
- **The WebAssembly client talks only to the frontend (BFF).** It never calls the backend and never holds secrets.
- **The frontend talks to the backend only over HTTP** with a typed `HttpClient` and a service token.
- **The backend is private.** Never publish its port in production. Every backend endpoint requires a valid service token (deny by default).
- **User identity reaches the backend only as the `uid` claim** in the service token. Never from URL, query or body.
- **Only the backend touches the database and the email service.** The frontend has no database or SMTP credentials.
- Secrets live only where needed: provider OAuth secrets and cookie keys in the frontend; database, SMTP and token validation keys in the backend.
- Organize by feature (`Features/Checklist`, `Features/Export`), not by layer.
- No business logic in `.razor` files. Components call services.
- Expected failures → result types. Exceptions only for unexpected errors.
- Minimal API endpoints grouped per feature with `MapGroup`. Return typed results.
- BFF endpoints stay thin: authorize the user, forward to the backend, map errors. No business logic.

## 4. Game data (seed JSON)

- **Item keys are permanent.** Format: `<game>.<category>.<slug>`, e.g. `ffx.al-bhed-primers.vol-01`.
  - Never rename, reuse or delete a key. Use `"deprecated": true` instead.
- Lowercase kebab-case slugs. ASCII only in keys.
- JSON Schema validated in CI: required fields, unique keys, valid prerequisites, known chapters/editions.
- Required on every item: `key`, `name`, `kind`, `spoiler`, `editions` (or empty = all), `missableAfter` (or `null`).
- Content:
  - Notes in our own words. Never copy guide text (Game8, Jegged, GameFAQs, etc.).
  - Every external source gets an `refs` entry with license (e.g. CC BY-SA for Fandom wikis).
  - No extracted game images or audio in the repo or the app.
- Data changes go in their own PRs with the `data:` prefix.

## 5. Export file format

- Always include `formatVersion`.
- Old files must always import. Each format change ships a migration step (v1 → v2 → …).
- Store item keys only, never database IDs.
- Treat imports as untrusted: max 1 MB, schema validation, unknown keys ignored with a warning.

## 6. Database (EF Core + PostgreSQL)

- snake_case names via `EFCore.NamingConventions`.
- One migration per PR. Descriptive name. Review the generated SQL.
- `AsNoTracking()` for reads. Cache static game data in memory.
- One database `companion`, schemas `app` and `identity`, used only by the backend. A runtime user (data read/write only) and a migrations user (schema owner).
- No raw SQL with string concatenation. Parameters only.

## 7. UI and theming (Blazor + Tailwind)

- Components use theme tokens (Tailwind classes mapped to CSS variables). No hard-coded colors.
- Every component must work in every theme.
- Mobile-first. Tap targets ≥ 44 px.
- Explicit render mode on every page (`@rendermode`), static SSR by default.
- Accessibility: WCAG 2.2 AA.
  - Keyboard navigation, visible focus.
  - Contrast checked per theme.
  - Never color alone for status: done/missed/missable also use icon or text.
  - Respect `prefers-reduced-motion`.
- UI strings in resource files from day one (English first).

## 8. Security

- No secrets in git. Use `dotnet user-secrets` or `.env` (git-ignored). Secret scanning in CI (gitleaks).
- Dependabot for NuGet and Docker images. `dotnet list package --vulnerable` in CI.
- HTTPS everywhere. Security headers: CSP, HSTS, `X-Content-Type-Options`, `Referrer-Policy`.
- Secure, HttpOnly, SameSite cookies.
- Validate all input. Antiforgery on forms and on cookie-authenticated BFF endpoints.
- **Service tokens:** JWT signed by the frontend, `aud` = backend, `exp` ≤ 5 min, scoped (`game-data.read`, `progress.write`, …). Signing keys only in secrets. Rotate keys; backend accepts current + previous key.
- Passwords are **hashed** by Identity in the backend (PBKDF2), never encrypted or logged. They travel frontend → backend only over the private network.
- Email sending: backend only, allowed templates only, rate-limited. No endpoint accepts arbitrary email content.
- No personal data in logs: no emails, tokens, IPs, OAuth payloads.

## 9. Testing

- Domain rules fully covered: completion %, missables, prerequisites, import/migration.
- Every bug fix includes a regression test.
- Testcontainers for PostgreSQL. No EF in-memory provider.
- Playwright smoke tests for 3–5 key flows.
- Deterministic tests: `TimeProvider`, no shared state, no real network.
- Test names: `Method_Scenario_ExpectedResult`.

## 10. Logging and observability

- Structured logs with message templates: `LogInformation("Imported {Count} items", count)`. Never interpolation.
- Levels:
  - `Error`: needs action.
  - `Warning`: unexpected but handled.
  - `Information`: key business events only.
  - `Debug`/`Trace`: local only.
- Production: `Information` minimum, sampled traces, no request/response bodies.
- Log budget: stay well under Grafana Cloud Free limits. Alert at 50% usage.
- **Telemetry path:** services → OpenTelemetry Collector (private) → Grafana. Services never call Grafana directly and never hold Grafana credentials.
- Browser errors go to the frontend's rate-limited `/bff/client-errors` endpoint. The WebAssembly client never sends telemetry anywhere else.

## 11. Docker and local development

- `docker compose up` works on a clean machine. README quickstart is tested.
- The backend has no published port in the default compose file. A `docker-compose.override.yml` may expose it on `127.0.0.1` for debugging only.
- Pinned image versions. Never `:latest`.
- Health checks on every service. Named volumes for PostgreSQL (and Grafana if data should persist).
- Seed data loads automatically in development.

## 12. Definition of Done

- [ ] CI green: build, format, tests, seed validation.
- [ ] Tests added or updated.
- [ ] Works at mobile width and with keyboard only.
- [ ] No new warnings.
- [ ] No secrets or personal data in code or logs.
- [ ] Seed keys unchanged (only additions or `deprecated`).
- [ ] ADR if a decision was made.
- [ ] Docs updated if behavior changed.

## 13. Legal and content

- Footer: "FINAL FANTASY is a registered trademark of Square Enix Holdings Co., Ltd. Fan project, not affiliated."
- Non-commercial. No ads, no paid features.
- CC BY-SA attribution wherever wiki-derived content is used.
- Respect third-party licenses in tools (check before reusing code).

## 14. Design principles

Apply these with judgment. If two conflict, prefer the simpler option.

- **Separation of Concerns:** one project, one area.
- **SOLID:**
  - Single Responsibility: a class has one reason to change.
  - Open/Closed: add a new game through data and a theme, not by changing shared code.
  - Liskov Substitution: an implementation honors its interface's contract.
  - Interface Segregation: small interfaces, shaped for the caller.
  - Dependency Inversion: dependencies point toward Domain. Add an interface only for a real boundary or a second implementation.
- **DRY:** one source of truth per rule or fact. Wait for the third real copy before extracting.
- **KISS:** the simplest design that works. No repository layer over EF Core, mediator or CQRS without a concrete need.
- **YAGNI:** build for the current plan task, not for later phases.
- **Functional core, imperative shell:** Domain stays pure, with no I/O, clock or logging. Services at the edges do the rest.
- **Least privilege:** each service, database user and token gets only the access it needs.
- **Secure by default:** deny first. Opening access is a deliberate step.
- **Fail fast:** validate configuration at startup and input at the boundary.
- **Composition over inheritance:** build from small parts, not base-class chains.
