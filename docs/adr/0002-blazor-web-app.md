# ADR 0002: Blazor Web App with static SSR + Interactive WebAssembly

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

We want one language (C#), fast public pages, and a checklist that works offline and without a server for guests.

## Decision

The frontend is a Blazor Web App. Static SSR for public pages. Interactive WebAssembly for interactive pages. No Interactive Server. The WebAssembly client talks only to the frontend's BFF endpoints (see ADR 0011).

## Consequences

- Good: shared C# models and logic, offline-capable checklist, server can scale to zero.
- Bad: larger first download for the WASM part (cached afterwards).

## Alternatives considered

- React/Svelte + API: better JS ecosystem, but two languages.
- Interactive Server: needs a live connection; bad on mobile networks and for scale-to-zero.
- Interactive Auto: more complex; not needed.
