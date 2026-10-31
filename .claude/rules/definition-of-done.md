# Definition of Done

Before saying a task is done, check:

- [ ] Builds with no warnings. `dotnet format --verify-no-changes` passes.
- [ ] Tests added/updated and `dotnet test` passes.
- [ ] Seed validation passes (if `data/` changed). Keys unchanged except additions/`deprecated`.
- [ ] UI works at mobile width and with keyboard only (if UI changed).
- [ ] No secrets or personal data in code or logs.
- [ ] ADR added if a decision was made.
- [ ] Docs updated if behavior changed.

If something couldn't be checked, say which and why.
