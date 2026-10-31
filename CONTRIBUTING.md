# Contributing

Short version of the rules. Full guidelines: [docs/05-guidelines.md](docs/05-guidelines.md).

## Workflow

1. Branch from `main`: `feat/...`, `fix/...`, `data/...`, `docs/...`, `chore/...`.
2. Keep branches short-lived: one plan subtask per PR, about 10 files and 300 changed lines at most. Each PR must be a stable release on its own.
3. Use [Conventional Commits](https://www.conventionalcommits.org/):
   `feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`, `data:` (seed JSON changes).
4. Open a pull request. CI must be green.
5. Squash-merge. No direct pushes to `main` (GitHub enforcement comes in P6.1.3).

## Decisions

Any decision that is hard to reverse gets an ADR in [docs/adr](docs/adr).
Copy [docs/adr/0000-template.md](docs/adr/0000-template.md).

## Definition of Done

- [ ] CI green: build, `dotnet format`, tests, seed validation.
- [ ] Tests added or updated (bug fix → regression test).
- [ ] Works at mobile width and with keyboard only.
- [ ] No new warnings.
- [ ] No secrets, tokens or personal data in code or logs.
- [ ] Seed keys unchanged (only additions or `deprecated`).
- [ ] ADR added if a decision was made.
- [ ] Docs/README updated if behavior changed.

## Game data rules (short)

- Item keys are permanent: `<game>.<category>.<slug>`. Never rename, reuse or delete.
- Write notes in your own words. Never copy guide text.
- Add attribution for any external source.
- No extracted game images or audio.

## Local setup

See the Quickstart in [README.md](README.md).
