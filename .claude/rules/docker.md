---
paths:
  - "docker-compose*.yml"
  - "**/Dockerfile"
  - "**/*.Dockerfile"
  - ".dockerignore"
---

# Docker and local development

- `docker compose up` must work on a clean machine.
- Services: `frontend`, `backend`, `postgres`, `otel-lgtm`; `mailpit` from Phase 5.
- Networks: `frontend` reaches only `backend` and the Collector (`otel-lgtm` locally). `postgres` and `mailpit` are on the backend network only.
- Grafana UI port published on `127.0.0.1` only.
- Persist frontend Data Protection keys in a named volume.
- `backend` is on the internal network only, with no published port. A `docker-compose.override.yml` may expose it on `127.0.0.1` for debugging.
- One Dockerfile per service (`src/Companion.Frontend/Dockerfile`, `src/Companion.Backend.Api/Dockerfile`).
- Pin image versions. Never `:latest`.
- Health checks on every service. `depends_on` with `condition: service_healthy`.
- Named volume for PostgreSQL data.
- Secrets via `.env` (git-ignored); keep `.env.example` up to date.
- Multi-stage Dockerfile, non-root runtime user.
