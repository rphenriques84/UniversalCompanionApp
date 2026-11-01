# 07 – Domain model

Game-agnostic. FFX is the first data set. New games add data, not schema.
nEverything here except "Seed files" is a draft. P1.7.6 replaces it.

## Concepts

| Concept | Meaning |
|---|---|
| **Game** | e.g. `ffx`, later `ffx-2` |
| **Edition** | Version with different content (`ps2-na`, `international`, `hd`) |
| **Chapter** | Ordered story checkpoint. Drives missables and spoilers. |
| **Category** | Group of items, can nest (Celestial Weapons > Caladbolg) |
| **CompletionItem** | One thing to do. Checkbox or counter (`TargetCount`). |
| **Prerequisite** | Item A needs item B (e.g. weapon needs crest + sigil) |
| **Playthrough** | One run of a game in one edition. Guests keep it in the browser. |
| **ItemProgress** | Progress on one item in one playthrough |

The model lives in `Companion.Domain`, shared by the backend and the WebAssembly client.
API DTOs live in `Companion.Contracts`. On the backend, `Playthrough.UserId` is the opaque user ID from the service token's `uid` claim.

## Rules

- `CompletionItem.Key` is permanent (see [05-guidelines.md](05-guidelines.md#4-game-data-seed-json)).
- Completion % = weighted average of category completion. Counters count as `min(Count, Target) / Target`.
- Items not in the playthrough's edition are excluded from totals.
- Items marked `Missed` stay in totals (100% becomes impossible, and the UI says so).
- An item is **missable soon** if not done and `MissableAfterChapter` is within the look-ahead from the current chapter.
- Spoiler items past the current chapter are hidden unless the user opts in.

## C# model (draft)

```csharp
// Game-agnostic 100% completion model. FFX first, other games later.
// Plain classes. .NET 10.

namespace Companion.Domain;

// ---------- Static game data (loaded from versioned JSON, read-only at runtime) ----------

public class Game
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";          // "ffx", "ffx-2", "ff9"
    public string Name { get; set; } = "";
    public List<GameEdition> Editions { get; set; } = [];
    public List<Chapter> Chapters { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}

/// Content differs by edition (e.g. Dark Aeons / Penance only in International & HD).
public class GameEdition
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Code { get; set; } = "";          // "ps2-na", "international", "hd"
    public string Name { get; set; } = "";
}

/// Ordered story checkpoints. Used for "missable after" and spoiler gating.
public class Chapter
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = "";          // "Besaid", "Luca", "Point of No Return"
    public bool IsPointOfNoReturn { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int? ParentId { get; set; }               // "Celestial Weapons" > "Caladbolg"
    public string Slug { get; set; } = "";           // "al-bhed-primers"
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public decimal Weight { get; set; } = 1;         // contribution to overall %
}

public enum ItemKind { Collectible, Quest, Boss, Minigame, Trophy, Treasure, Ability, Counter }

public class CompletionItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Key { get; set; } = "";            // stable: "ffx.primer.01" — never reuse
    public string Name { get; set; } = "";
    public ItemKind Kind { get; set; }
    public int Order { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }               // your own wording, not copied guides

    public int? AvailableFromChapterId { get; set; }
    public int? MissableAfterChapterId { get; set; } // null = never missable
    public bool IsSpoiler { get; set; }

    /// For counters ("Dodge 200 bolts", "Capture 10 of each fiend"). Null = checkbox.
    public int? TargetCount { get; set; }

    /// Empty = all editions.
    public List<GameEdition> Editions { get; set; } = [];
    public List<ItemPrerequisite> Prerequisites { get; set; } = [];
    public List<ExternalRef> Refs { get; set; } = [];
}

/// "Caladbolg needs Sun Crest + Sun Sigil + Celestial Mirror".
public class ItemPrerequisite
{
    public int ItemId { get; set; }
    public int RequiresItemId { get; set; }
}

/// Attribution / link-outs (wiki page with CC BY-SA credit, video, map).
public class ExternalRef
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public string? License { get; set; }             // "CC BY-SA 3.0"
}

// ---------- User data ----------

public class Playthrough
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }                 // server-side only (Phase 5); guests keep playthroughs in the browser
    public int GameId { get; set; }
    public int EditionId { get; set; }
    public string Name { get; set; } = "";           // "HD 100% run"
    public int? CurrentChapterId { get; set; }       // drives missable warnings + spoilers
    public DateTimeOffset CreatedAt { get; set; }
    public List<ItemProgress> Progress { get; set; } = [];
}

public class ItemProgress
{
    public Guid PlaythroughId { get; set; }
    public int ItemId { get; set; }
    public int Count { get; set; }                   // checkbox items use 0/1
    public bool Done { get; set; }
    public bool Missed { get; set; }                 // user marks as permanently missed
    public DateTimeOffset UpdatedAt { get; set; }
    public string? Source { get; set; }              // "manual", "file-import", "sync", "save-import"
}

// ---------- Services ----------

public record CategoryProgress(int CategoryId, string Name, int Done, int Total, decimal Percent);

public record MissableWarning(int ItemId, string Name, string MissableAfter);

public interface ICompletionService
{
    Task<decimal> GetOverallPercentAsync(Guid playthroughId, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryProgress>> GetByCategoryAsync(Guid playthroughId, CancellationToken ct = default);

    /// Items still open that become missable at or before the next N chapters.
    Task<IReadOnlyList<MissableWarning>> GetUpcomingMissablesAsync(Guid playthroughId, int lookAheadChapters = 1, CancellationToken ct = default);
}

/// Phase 7: per-game save parsers plug in here.
public interface ISaveImporter
{
    string GameSlug { get; }
    bool CanRead(Stream save);
    Task<IReadOnlyDictionary<string, int>> ReadProgressAsync(Stream save, CancellationToken ct = default); // Item.Key -> count
}
```

## Seed JSON shape (draft)

```json
{
  "game": "ffx",
  "category": "al-bhed-primers",
  "items": [
    {
      "key": "ffx.al-bhed-primers.vol-01",
      "name": "Al Bhed Primer Vol. I",
      "kind": "Collectible",
      "order": 1,
      "location": "(write in our own words)",
      "availableFrom": "ffx.chapter.salvage-ship",
      "missableAfter": "ffx.chapter.salvage-ship",
      "spoiler": false,
      "editions": [],
      "refs": []
    }
  ]
}
```

> Locations and chapters above are placeholders. Verify every entry in-game or against attributed sources.
