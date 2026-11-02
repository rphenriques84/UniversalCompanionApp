# Universal Companion App

A web app for tracking **100% completion** in video games.
It starts with **Final Fantasy X (HD Remaster)**. More games come later.

> Status: **in development (Phase 0)**. See [docs/04-development-plan.md](docs/04-development-plan.md).

## What it does

- Checklists per game: collectibles, side quests, bosses, minigames, trophies.
- Counters for "do X times" goals.
- Completion % per category and overall.
- Missable warnings before points of no return.
- Spoiler protection based on your current chapter.
- **Guest mode by default.** Progress is saved in your browser.
- **Export/import a file** to keep or move your progress.
- Optional accounts later (external sign-in or email/password) with server sync.
- Game-themed visuals per game.

## Tech stack (summary)

| Area | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| Services | Public frontend (BFF) + private backend API |
| Frontend | Blazor Web App (static SSR + Interactive WebAssembly) |
| Backend | ASP.NET Core Minimal APIs, JWT service-token auth |
| Styling | Tailwind CSS, CSS-variable themes per game |
| Database | PostgreSQL + EF Core (Npgsql), from Phase 5 |
| Game data | Versioned JSON in this repo, loaded into memory by the backend |
| Auth (later) | ASP.NET Core Identity in the backend, sign-in handled by the frontend: external logins + optional email/password |
| Observability | OpenTelemetry → Grafana (local `otel-lgtm`, later Grafana Cloud) |
| Local environment | Docker Compose |
| Tests | xUnit, bUnit, Playwright; Testcontainers from Phase 5 |
| CI | GitHub Actions |
| Hosting | Deferred. See [docs/11-hosting-options.md](docs/11-hosting-options.md) |

Details: [docs/02-tech-stack.md](docs/02-tech-stack.md).

## Documentation

Start at [docs/README.md](docs/README.md).

## Quickstart

```bash
cp .env.example .env
dotnet tools/generate-service-keys.cs
docker compose up
```

Then open the frontend at `http://localhost:8080` and Grafana at `http://localhost:3000`.
The backend API is private (internal Docker network only).

## Debugging

The frontend and backend run directly, without Docker. First time only:

```bash
dotnet tools/setup-dev-secrets.cs
```

In Visual Studio, choose the **Frontend + Backend** startup profile and press F5.
It opens the frontend at `http://localhost:5110`, with the backend at `http://localhost:5147`.
To call the BFF endpoints from the browser, open `http://localhost:5110/scalar`.
To call the backend directly, open `http://localhost:5147/scalar`. Backend calls need a service token, valid for 5 minutes. Paste it into the page's Bearer token field:

```bash
dotnet tools/dev-service-token.cs
```

These pages exist only in Development, so they are not in Docker, which runs in Production.

Other tools use two terminals:

```bash
dotnet run --project src/Companion.Backend.Api --launch-profile http
dotnet run --project src/Companion.Frontend --launch-profile http
```

## Legal

FINAL FANTASY is a registered trademark of Square Enix Holdings Co., Ltd.
This is a non-commercial fan project. It is not affiliated with or endorsed by Square Enix.
Game data belongs to its owners. See [docs/06-data-sources.md](docs/06-data-sources.md).
