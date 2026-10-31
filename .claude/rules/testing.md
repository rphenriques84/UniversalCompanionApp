---
paths:
  - "tests/**"
---

# Testing

- xUnit for domain; bUnit for components; Testcontainers (real PostgreSQL) for data from Phase 5; Playwright for a few smoke flows.
- Never use the EF Core in-memory provider.
- Backend tests cover service-token auth: missing/expired/wrong-audience tokens are rejected; `uid` comes only from the token.
- Frontend BFF tests mock the backend with a fake `HttpMessageHandler`.
- Domain rules need full coverage: completion %, missables, prerequisites, spoilers, import and migrations.
- Every bug fix adds a regression test.
- Deterministic tests: `FakeTimeProvider`, no shared state, no real network.
- Test names: `Method_Scenario_ExpectedResult`. Arrange/Act/Assert.
- Run `dotnet test` before saying work is done. Report failures honestly.
