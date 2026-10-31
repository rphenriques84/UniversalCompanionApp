---
paths:
  - "src/Companion.Backend.Data/**"
  - "**/Migrations/**"
---

# Database (EF Core + PostgreSQL, from Phase 5)

- Only the backend connects to PostgreSQL. One database `companion`, schemas `app` (progress) and `identity` (Identity Core).
- Game data stays in JSON, in memory (ADR 0013). Reference items by their permanent key.
- snake_case naming via `EFCore.NamingConventions`.
- One migration per PR, descriptive name. Show the generated SQL for review.
- Never edit an applied migration. Add a new one.
- `AsNoTracking()` for read-only queries. Static game data is cached in memory.
- No raw SQL with string concatenation. Parameters only.
- Runtime DB user has data read/write only. Migrations run with a separate owner user.
- Identity Core (backend, Phase 5): `RequireUniqueEmail = false` + filtered unique index on `normalized_email WHERE normalized_email IS NOT NULL`.
