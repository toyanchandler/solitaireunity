# Solitaire Module Folder Structure

Physical folders show **ownership by feature**. Namespaces stay on the original layer (`Runtime`, `Controllers`, `Views`, etc.) so prefab script GUIDs and call sites do not break when files move.

Related:

- [SOLITAIRE_MODULE_RUNTIME_WIRING.md](./SOLITAIRE_MODULE_RUNTIME_WIRING.md) — scene/prefab wiring
- [HOW_TO_REFACTOR_WITH_LOGIC_PARTIALS.md](../02_How_To/HOW_TO_REFACTOR_WITH_LOGIC_PARTIALS.md) — `*Logic` partial split pattern

---

## Why not one `Runtime/` dump folder?

Before refactor, `Runtime/` held ~35 unrelated scripts (bootstrap, board state, hints, layout math, registry). That made discovery hard and encouraged “drop the next file here” growth.

Rule: **a folder should answer one question**. If you cannot describe the folder in one sentence, split it.

---

## Target layout

```text
Assets/_Game/Scripts/Project/SolitaireModule/
├── Data/                          ScriptableObjects, shared types
├── Rules/                         Pure move/pile rules (no Unity lifecycle)
│   └── SolitairePileMoveRules.*   Source/target/input rule partials
├── Input/                         Pointer, hit test, drop target resolve
│
├── Bootstrap/                     Composition root + registration + level start
├── Board/                         Board state, piles, snapshots
├── Moves/                         Move service, executor, game flow
├── Hints/                         Hint service + SolitaireHintLogic partials
├── Layout/                        Responsive layout calculator partials
├── Registry/                      View registry + runtime context bag
├── Debug/                         Debug scenarios (runtime applier + panel)
│
├── Controllers/                   Gameplay orchestration only (thin)
├── Presentation/
│   ├── Deal/                      Initial deal animation
│   ├── Layout/                    Pile layout + responsive board
│   ├── Drag/                      Drag visuals + selection
│   └── Win/                       Win celebration
│
├── Views/
│   ├── Card/                      CardView partials + card components
│   ├── Slots/                     Tableau/foundation/stock slot anchors
│   ├── Board/                     Drag layer, backdrop, hint presenter
│   └── Fx/                        Deck ripple, pulse ring
│
└── Editor/                        Scene builder, benchmarks, WebGL tools
```

---

## Folder ownership (one sentence each)

| Folder | Owns |
|--------|------|
| `Data/` | Config assets and serializable/domain types |
| `Rules/` | “Can this move execute?” pure rules |
| `Input/` | Screen → board hit → drop target resolution |
| `Bootstrap/` | Who starts the module and who registers scene objects |
| `Board/` | Authoritative pile/card board model |
| `Moves/` | Applying moves and exposing move/hint queries |
| `Hints/` | Collecting and ranking legal hints |
| `Layout/` | Camera/viewport → card size and slot positions |
| `Registry/` | Resolved `CardView` / slot lookups for the session |
| `Debug/` | Editor/runtime debug scenario tooling |
| `Controllers/` | Orchestrate deck, input, layout, win — no layout math |
| `Presentation/Deal/` | Deal animation sequencing |
| `Presentation/Layout/` | Visual pile positions and responsive reflow |
| `Presentation/Drag/` | Drag follower and selection chrome |
| `Presentation/Win/` | Win feedback presentation |
| `Views/Card/` | Single card prefab behaviour |
| `Views/Slots/` | Empty pile anchor behaviour |
| `Views/Board/` | Board-level view helpers (drag root, backdrop) |
| `Views/Fx/` | Optional card/board VFX views |
| `Editor/` | Build scene, wire HUD, run benchmarks |

---

## File counts (current)

| Folder | `.cs` files |
|--------|-------------|
| `Views/` (all subfolders) | 25 |
| `Editor/` | 10 |
| `Presentation/` (all subfolders) | 9 |
| `Controllers/` | 9 |
| `Layout/` | 8 |
| `Hints/` | 8 |
| `Bootstrap/` | 7 |
| `Board/` | 6 |
| `Rules/` | 5 |
| `Moves/` | 5 |
| `Data/` | 5 |
| `Input/` | 4 |
| `Debug/` | 3 |
| `Registry/` | 2 |

Max folder size target: **~12 gameplay files** at top level; use subfolders or `*Logic` partials beyond that.

---

## Namespace vs folder

| Physical folder | Typical namespace | Example |
|-----------------|-------------------|---------|
| `Bootstrap/` | `...Runtime` | `SolitaireModuleBootstrap` |
| `Board/` | `...Runtime` | `SolitaireBoardState` |
| `Moves/` | `...Runtime` | `SolitaireMoveService` |
| `Hints/` | `...Runtime` | `SolitaireHintLogic` |
| `Layout/` | `...Runtime` | `SolitaireBoardLayoutCalculator` |
| `Registry/` | `...Runtime` | `SolitaireViewRegistry` |
| `Controllers/` | `...Controllers` | `SolitaireDeckController` |
| `Views/Card/` | `...Views` | `CardView` |
| `Presentation/Layout/` | `...Presentation` | `SolitairePileLayoutPresenter` |

Folder = where to look. Namespace = compile-time layer. They do not have to match byte-for-byte.

---

## Where to put a new script

```text
New ScriptableObject config        → Data/
New pure pile/move rule            → Rules/
New pointer/drop behaviour         → Input/
New startup/registration piece     → Bootstrap/
New board model type               → Board/
New move apply or query service    → Moves/
New hint rule                      → Hints/ (+ partial if file grows)
New responsive layout math         → Layout/ (+ partial if file grows)
New session lookup/registry        → Registry/
New debug-only scenario tool       → Debug/
New deck/input/win orchestration   → Controllers/ (keep thin; logic → *Logic)
New animation/layout presenter     → Presentation/<Deal|Layout|Drag|Win>/
New card prefab component          → Views/Card/
New slot prefab component          → Views/Slots/
New board-level view               → Views/Board/
New VFX view                       → Views/Fx/
Editor-only tool                   → Editor/
```

If logic exceeds ~200 lines, add `ThingLogic.cs` + `ThingLogic.*.cs` partials **inside the same domain folder** (see `Hints/`, `Layout/`, `Views/Card/`).

---

## Anti-patterns

| Do not | Do instead |
|--------|------------|
| Add another file to a 20+ file flat folder | Create or use a domain subfolder |
| Put layout math in `Controllers/` | `Layout/` or `Presentation/Layout/` |
| Put hint rules in `SolitaireDeckController` | `Hints/SolitaireHintLogic.*.cs` |
| Rename namespaces when moving files | Keep namespace; update docs only |
| Create `Runtime2/` or `Misc/` | Pick a domain row from the table above |

---

## Migration note (2026)

The former flat `Runtime/` folder was split into `Bootstrap/`, `Board/`, `Moves/`, `Hints/`, `Layout/`, `Registry/`, and `Debug/`. `Presentation/` and `Views/` gained feature subfolders. **Prefab and scene references are preserved** via moved `.meta` GUIDs; no namespace churn.
