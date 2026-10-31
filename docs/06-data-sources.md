# 06 – Data sources and licensing (FFX)

FFX has no official API. We build and host our own dataset.

## Sources

### 1. Game files (raw data)

- **Tool:** [Karifean/FFXDataParser](https://github.com/Karifean/FFXDataParser) (Java, Maven).
- **Input:** files from your own legal copy of FFX/X-2 HD Remaster, extracted with `vbfextract` from `FFX_Data.vbf`.
- **Can read:** commands and abilities, key items, monsters (incl. localized names, scan text), battles and encounter tables, treasures, gear/item shops, Sphere Grid node types and layout, weapons, PC stats, events.
- **Output:** console text. Our `Companion.SeedBuilder` converts it to JSON.
- **License:** not stated in the repo. Ask the author before reusing its code. Running it locally is fine.
- **Rule:** game files and raw extraction output are never committed (`.gitignore`).

### 2. Hand-curated checklist

- Primers, Jecht Spheres, Celestial Weapons, aeons, Dark Aeons, Monster Arena, Blitzball, minigames, trophies, missables.
- Written in our own words.
- **Reference:** Final Fantasy Wiki (Fandom). Text is **CC BY-SA**: credit it, and adapted text must stay CC BY-SA.
- **Do not copy:** Game8 (explicitly forbids reuse), Jegged, GameFAQs guides, Prima. Facts are fine; their text and structure are not.
- **Backbone:** the official HD trophy list (e.g. all 26 Al Bhed Primers, all Jecht Spheres, complete all Sphere Grids, defeat Penance).

### 3. Save files (Phase 7, optional)

- Existing tools that read PC saves: FFXED (PCSX2 forums), BitPatty's Checksum Repair Tool (AGPL-3.0), Farplane (memory editor, desktop only).
- Progress flags are poorly documented. Research needed before committing to this feature.

## Licensing summary

| Item | Owner | Our use |
|---|---|---|
| Game data, names, story | Square Enix | Non-commercial fan use, with trademark notice |
| Game images, audio | Square Enix | **Not used** |
| Fandom wiki text | Contributors, CC BY-SA | Reference; attribute if adapted |
| Commercial guide text | Their publishers | **Not used** |
| FFXDataParser | Its author (license unstated) | Run locally only |

## Attribution in data

Each item can carry `refs`:

```json
"refs": [
  { "label": "Final Fantasy Wiki: Al Bhed Primers", "url": "https://finalfantasy.fandom.com/wiki/Al_Bhed_Primer", "license": "CC BY-SA 3.0" }
]
```

The UI shows these as "Sources" on the item.
