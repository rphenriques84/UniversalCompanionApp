---
paths:
  - "**/Export/**"
  - "**/Import/**"
  - "data/schema/export*"
---

# Export file format

- Spec: `docs/09-export-file-format.md`.
- Always include `formatVersion`. Store item keys only, never database IDs.
- Changing the format: bump `formatVersion`, add a `vN → vN+1` migration in Domain, add a test using a real old file, update the schema and the doc.
- Old files must always import.
- Imports are untrusted: max 1 MB, schema validation, unknown game rejected, unknown keys ignored with a warning, text length-limited and encoded on display.
- Merge rule: highest `count`, `done` if either is done, latest `updatedAt` for other fields.
