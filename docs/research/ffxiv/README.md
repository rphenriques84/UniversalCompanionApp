# Research: FFXIV data sources

Early research from when the app targeted FFXIV (Free Company/static manager idea).
Kept for when FFXIV is added as a game. **Not part of the current plan.**
Checked October 2026. Re-verify before use.

## XIVAPI v2 – static game data

- Every game sheet (Item, Action, Recipe, ClassJob, …) for every patch since 7.0, plus icons/maps as PNG/JPG/WebP.
- No player data (no characters, FCs, inventories).
- Endpoints: `/api/sheet/{Sheet}/{row}`, `/api/sheet/{Sheet}?rows=…&after=…&limit=…`, `/api/search?sheets=…&query=…`, `/api/asset?path=…&format=png`, `/api/asset/map/{id}/{index}`.
- Always pass `fields=` (full rows can exceed 80 kB). Pin `version` and `schema` for stability.
- No API key. No documented rate limit: be polite, back off on 429.
- Assets: hotlinking encouraged; respects ETag/If-None-Match.
- Caching: sync needed sheets to your DB once per patch.
- OpenAPI reference: https://v2.xivapi.com/api/docs
- Postman: [XIVAPI-v2.postman_collection.json](XIVAPI-v2.postman_collection.json)

## Universalis – market board (crowdsourced)

- Current listings, history, aggregated prices, marketable items, worlds, data centers, tax rates. WebSocket for live events (BSON).
- Base: `https://universalis.app/api/v2/`. Up to 100 item IDs per call.
- Limits: 25 req/s (50 burst), max 8 simultaneous connections per IP.
- Check `lastUploadTime`: freshness depends on player uploads.
- Caching: aggregated 5–15 min, history 30–60 min, reference data 1 day.
- Postman: [Universalis-v2.postman_collection.json](Universalis-v2.postman_collection.json)

## NetStone – Lodestone scraper (.NET)

- NuGet `NetStone` (1.4.1, .NET Standard 2.1). Characters, FCs, linkshells, CWLS, PvP teams, plus search.
- One `LodestoneClient` (`await LodestoneClient.GetClientAsync()`) reused for all requests.
- Scrapes HTML: breaks on Lodestone layout changes until selectors are updated.
- No published rate limit. Keep ~1 req/s, cache 6–24 h, refresh in background.
- **License: AGPL-3.0.** For a hosted web app this can require publishing your source. Isolate it in a separate open-source service or open-source the app.
- Postman (raw Lodestone pages NetStone parses): [Lodestone-NetStone.postman_collection.json](Lodestone-NetStone.postman_collection.json)

## Legal

Game data © Square Enix. Non-commercial use under SE's Materials Usage License.
