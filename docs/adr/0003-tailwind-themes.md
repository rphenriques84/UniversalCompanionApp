# ADR 0003: Tailwind CSS with per-game themes

- **Status:** Accepted
- **Date:** 2026-10-30

## Context

Each game should have its own visual theme. Component libraries impose a look that is hard to override.

## Decision

Use Tailwind CSS. Theme tokens are CSS variables, switched with `data-theme` on `<html>`. Build with the Tailwind standalone CLI from MSBuild.

## Consequences

- Good: full control, one theme file per game.
- Bad: we build our own components.

## Alternatives considered

- MudBlazor: fast start, Material look.
- FluentUI Blazor: Fluent look.
