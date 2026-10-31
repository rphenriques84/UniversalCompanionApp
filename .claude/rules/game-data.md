---
paths:
  - "data/**"
  - "tools/Companion.SeedBuilder/**"
---

# Game data (seed JSON)

- Item keys are **permanent**: `<game>.<category>.<slug>` (e.g. `ffx.al-bhed-primers.vol-01`). Lowercase kebab-case, ASCII.
- Never rename, reuse or delete a key. Mark obsolete items `"deprecated": true`.
- Chapter keys: `<game>.chapter.<slug>`. Edition codes: `ps2-na`, `international`, `hd`.
- Required per item: `key`, `name`, `kind`, `spoiler`, `editions` (empty = all), `missableAfter` (chapter key or `null`).
- Every file must validate against `data/schema/*.schema.json`. Run the validator after edits.
- Notes in our own words. Never copy text from commercial guides (Game8, Jegged, GameFAQs, Prima).
- External sources go in `refs` with `label`, `url`, `license` (Fandom wikis: CC BY-SA).
- Never add extracted game images, audio or raw game files.
- If unsure about a fact (location, chapter, missable), mark it `"verified": false` and tell the user. Don't guess silently.
