# 09 – Export file format

The export file lets guests keep and move progress. It must keep working forever.

## File

- Extension: `.ucompanion.json` (e.g. `ffx-hd-100-run.ucompanion.json`).
- Encoding: UTF-8 JSON.
- Max size accepted on import: **1 MB**.

## Version 1 (draft)

```json
{
  "formatVersion": 1,
  "app": "UniversalCompanionApp",
  "exportedAt": "2026-10-30T20:00:00Z",
  "playthroughs": [
    {
      "id": "6f1c2d1e-0d3b-4e8e-9f42-1a2b3c4d5e6f",
      "game": "ffx",
      "edition": "hd",
      "name": "HD 100% run",
      "currentChapter": "ffx.chapter.luca",
      "progress": {
        "ffx.al-bhed-primers.vol-01": { "count": 1, "done": true, "updatedAt": "2026-10-30T19:12:00Z" },
        "ffx.minigames.lightning-dodges": { "count": 57, "done": false, "updatedAt": "2026-10-30T19:30:00Z" },
        "ffx.jecht-spheres.besaid": { "count": 0, "done": false, "missed": true, "updatedAt": "2026-10-30T19:31:00Z" }
      }
    }
  ]
}
```

## Rules

- `formatVersion` is required.
- Only **item keys**, never database IDs.
- Every format change:
  1. Increments `formatVersion`.
  2. Adds a migration step `vN → vN+1` in Domain.
  3. Adds a test with a real old file.
- Old files must always import.

## Import

1. Check size (≤ 1 MB) and JSON validity.
2. Validate against `data/schema/export.v{N}.schema.json`.
3. Migrate to the current version.
4. Unknown game → reject with message. Unknown item keys → ignore and warn (count shown).
5. Ask: **Replace** or **Merge**.
   - Merge rule per item: highest `count`; `done` if either is done; latest `updatedAt` for other fields.
6. Save to browser storage (guest) or sync to server (signed in).

## Security

- The file is untrusted input. No code, no HTML rendering of its strings.
- Text fields (playthrough name) are length-limited and encoded on display.
