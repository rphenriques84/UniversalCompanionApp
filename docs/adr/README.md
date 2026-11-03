# Architecture Decision Records

| ADR | Title | Status |
|---|---|---|
| [0001](0001-dotnet-10-lts.md) | .NET 10 (LTS) as runtime | Accepted |
| [0002](0002-blazor-web-app.md) | Blazor Web App with static SSR + Interactive WebAssembly | Accepted |
| [0003](0003-tailwind-themes.md) | Tailwind CSS with per-game themes | Accepted |
| [0004](0004-postgresql.md) | PostgreSQL as database | Amended by 0013 |
| [0005](0005-docker-compose.md) | Docker Compose for local development | Accepted |
| [0006](0006-static-data-json.md) | Game data as versioned JSON in git | Amended by 0013 |
| [0007](0007-guest-first-export.md) | Guest-first with browser storage and export file | Accepted |
| [0008](0008-identity.md) | ASP.NET Core Identity with external logins and optional email/password | Accepted |
| [0009](0009-observability-grafana.md) | OpenTelemetry with Grafana locally and in production | Accepted |
| [0010](0010-hosting-deferred.md) | Defer hosting decision to Phase 6 | Accepted |
| [0011](0011-frontend-backend-split.md) | Public frontend (BFF) and private backend API | Accepted |
| [0012](0012-project-structure.md) | Six source projects, no extra layers | Accepted |
| [0013](0013-game-data-in-memory.md) | Serve game data from JSON, postpone PostgreSQL to Phase 5 | Accepted |
| [0014](0014-one-local-secrets-file.md) | One local secrets file | Accepted |

New ADR: copy [0000-template.md](0000-template.md), use the next number.
