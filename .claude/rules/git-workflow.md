# Git and workflow

- Trunk-based. `main` must always build and pass tests.
- Branch names: `feat/...`, `fix/...`, `data/...`, `docs/...`, `chore/...`.
- Reference development plan IDs (`docs/04-development-plan.md`, e.g. `P0.4.2`) in branch names, commits and PRs. Tick off subtasks in the plan when done.
- Conventional Commits: `feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`, `data:` (seed JSON).
- One concern per PR. Data changes in their own PR.
- One plan subtask per PR. Merge it before starting the next one.
- Keep each PR to about 15 files and 300 changed lines. Generated or template files don't count. If a subtask won't fit, split it in the plan first. If a PR must go over, say why in the PR.
- Each PR must be a stable release on its own: `main` could ship as it is. It builds, tests pass, `docker compose up` works and nothing half-done is exposed.
- Keep unrelated fixes, rule changes and refactors out of a feature PR. Give them their own PR.
- Squash-merge. No direct pushes to `main`.
- Hard-to-reverse decision → new ADR in `docs/adr/` from `0000-template.md`, next number, and add it to `docs/adr/README.md`.
- Only commit or push when the user asks.
