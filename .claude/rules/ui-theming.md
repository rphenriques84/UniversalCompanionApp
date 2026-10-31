---
paths:
  - "**/*.razor"
  - "**/*.razor.cs"
  - "**/*.css"
  - "**/tailwind*"
  - "themes/**"
---

# UI and theming (Blazor + Tailwind)

- Use theme tokens (Tailwind classes mapped to CSS variables). Never hard-code colors, fonts or radii in components.
- Every component must work in every theme (`data-theme` on `<html>`).
- Build our own components. Don't add component libraries.
- Mobile-first. Tap targets ≥ 44 px.
- Explicit `@rendermode` on interactive pages. Static SSR by default.
- Accessibility (WCAG 2.2 AA):
  - Keyboard reachable, visible focus, semantic HTML, labels on inputs.
  - Contrast checked per theme.
  - Never color alone for status: done/missed/missable also use icon or text.
  - Respect `prefers-reduced-motion`.
- UI strings in resource files (`.resx`), English first.
- Hide spoiler content past the current chapter unless the user opted in.
