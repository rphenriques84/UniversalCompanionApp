# Git and workflow

- Trunk-based. `main` must always build and pass tests.
- Branch names: `feat/...`, `fix/...`, `data/...`, `docs/...`, `chore/...`.
- Reference development plan IDs (`docs/04-development-plan.md`, e.g. `P0.4.2`) in branch names, commits and PRs. Tick off subtasks in the plan when done.
- Conventional Commits: `feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`, `data:` (seed JSON).
- One concern per PR. Data changes in their own PR.
- Squash-merge. No direct pushes to `main`.
- Hard-to-reverse decision → new ADR in `docs/adr/` from `0000-template.md`, next number, and add it to `docs/adr/README.md`.
- Only commit or push when the user asks.
