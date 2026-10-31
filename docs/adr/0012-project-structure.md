# ADR 0012: Six source projects, no extra layers

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

ADR 0011 splits the app into a public frontend and a private backend.
Domain rules must also run in the browser, and the frontend and backend may not reference each other.
This is a solo project, so each project must earn its place.

## Decision

Use six source projects. Each exists because of a real boundary:

- `Domain`: rules shared by browser and server. Depends on nothing.
- `Contracts`: HTTP data shapes shared by frontend and backend. Depends on nothing.
- `Frontend`: public host.
- `Frontend.Client`: WebAssembly code. Everything in it is public.
- `Backend.Api`: private host.
- `Backend.Data`: game data loader. EF Core and migrations from Phase 5 (ADR 0013).

Dependencies point toward `Domain`. Endpoints call Domain and `Backend.Data` directly, with no Application or Infrastructure layer.

## Consequences

- Good: boundaries are enforced by architecture tests; less code than layered designs; database code stays out of the public service.
- Bad: `Frontend` still mixes pages, BFF and sign-in, so feature folders must stay clean. One DTO set serves both backend → frontend and frontend → browser.
- Neutral: separate browser DTOs can be added if a field must stay internal.

## Alternatives considered

- Single project: can't keep database code out of the public service, or Domain light enough for the browser.
- Full Clean Architecture (Application and Infrastructure layers): adds pass-through code with no gain for this size.
- Merging `Backend.Data` into `Backend.Api`: would work, but data access and migrations change for different reasons than endpoints.
