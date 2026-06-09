# How To Refactor With Logic Partials

This guide documents the refactor pattern applied to large Solitaire module files: split decision logic into testable `internal static` helpers, keep Unity/scene wiring in thin host classes, and use `partial` files when a logic class grows past ~200 lines.

Related rules:

- [INTERNAL_STATIC_HELPERS_RULES.md](../01_Permanent_Project_Rules/INTERNAL_STATIC_HELPERS_RULES.md)
- [ADR_004_INTERNAL_STATIC_HELPERS.md](../06_Decision_Records/ADR_004_INTERNAL_STATIC_HELPERS.md)
- [SOLITAIRE_MODULE_RUNTIME_WIRING.md](../03_Hierarchy/SOLITAIRE_MODULE_RUNTIME_WIRING.md)
- [SOLITAIRE_MODULE_FOLDER_STRUCTURE.md](../03_Hierarchy/SOLITAIRE_MODULE_FOLDER_STRUCTURE.md)

---

## When to use this pattern

Use logic partials when a file has most of these symptoms:

- One class is 400+ lines and mixes Unity lifecycle with pure decisions
- Methods contain repeated `if (x == null) return` guards
- The same calculations appear in multiple public methods
- Unit testing is hard because logic is buried inside `MonoBehaviour`
- A single `*Logic.cs` file is itself becoming hard to navigate

Do **not** use this pattern when:

- Scene composition alone solves the task (prefer prefab/inspector wiring per `AGENTS.md`)
- The behavior needs lifecycle, serialized refs, or event subscription in the helper itself
- You would create a parallel manager/system instead of extracting pure rules

---

## Target architecture

```text
Host (MonoBehaviour or public static facade)
├── serialized fields / Unity API calls only
├── 1–3 line public methods that delegate
└── private "apply" methods that mutate components using logic output

*Logic (internal static partial class)
├── nested static groups by responsibility
├── pure guards, math, mapping, validation
├── small methods (ideally < 15 lines)
└── no scene search, no event subscribe, no hidden global state
```

### Responsibility split

| Stays in host | Moves to `*Logic` |
|---------------|-------------------|
| `Awake` / `OnEnable` / `OnDestroy` | Guard decisions (`ShouldSkip*`, `CanApply*`) |
| `GetComponent`, `transform`, `GameObject` creation | Sorting order math, layout math, hint scoring |
| Applying colors/sprites to `SpriteRenderer` | Move equality, pile ref building, cycle index |
| Inspector-serialized references | Validation messages, enum/state resolution |

---

## Naming conventions

### Logic file names

| Host | Logic root | Partial splits (when needed) |
|------|------------|------------------------------|
| `CardView.cs` | `CardViewLogic.cs` | single file (medium size) |
| `SolitaireHintService.cs` | `SolitaireHintLogic*.cs` | `.Validation`, `.Shared`, `.Foundation`, `.Waste`, `.Tableau`, `.Stock` |
| `SolitaireBoardLayoutCalculator.cs` | `SolitaireBoardLayoutCalculator.*.cs` | `.Core`, `.Constants`, `.Sizing`, `.Portrait`, `.Landscape`, `.ResultFactory` |

Pattern:

```text
<TypeName>Logic.cs
<TypeName>Logic.<Topic>.cs     // partial continuation
```

For `public static` calculators, the host itself is `partial`:

```csharp
public static partial class SolitaireBoardLayoutCalculator { ... }
```

### Nested static class names

Use behavior names already approved in project rules:

| Suffix / name | Purpose | Example |
|---------------|---------|---------|
| `Guard` | Early-exit predicates | `ShouldSkipRefresh`, `ShouldRegisterCard` |
| `Validation` | Input checks / error strings | `TryValidate`, `RequireCollectInputs` |
| `*Factory` | Build DTOs / results | `MoveFactory`, `ResultFactory` |
| `*Ops` / `*Applier` | Stateless apply helpers (optional) | `SpriteRendererOps` |
| Domain noun | Feature-specific rules | `FoundationHints`, `Portrait`, `DragShadow` |

Prefer guard methods over inline `if/else` chains:

```csharp
// Good
if (!Guard.ShouldSkipRefresh(isPresenting))
    return;

// Good
return ShouldClampToVerticalLimit(maxHeight, cardHeight)
    ? ClampToVerticalLimit(...)
    : new Vector2(cardWidth, cardHeight);
```

---

## Case study 1: `CardView`

**Location:** `Assets/_Game/Scripts/Project/SolitaireModule/Views/Card/`

### Before

- ~530 lines in one `MonoBehaviour`
- Drag shadow, selection highlight, layout, sorting, and feedback mixed together

### After

| File | Role |
|------|------|
| `CardView.cs` | Fields, lifecycle, public presentation API |
| `CardView.Setup.cs` | Component resolve, layout apply, sorting |
| `CardView.Drag.cs` | Drag input + drag visual state |
| `CardView.Visuals.cs` | Drag shadow + selection highlight `GameObject` setup |
| `CardView.Feedback.cs` | Pressed/reset scale feedback |
| `CardView.Editor.cs` | `OnValidate` inspector sync |
| `CardViewLogic.cs` | Constants, guards, identity, validation |
| `CardViewLogic.Layout.cs` | Scale/collider/highlight math |
| `CardViewLogic.Renderer.cs` | Sorting, sprite renderer ops, child lookup |
| `CardViewLogic.Drag.cs` | Face sprites, drag visual, drag shadow rules |
| `CardViewLogic.SelectionHighlight.cs` | Selection highlight resolve/configure |

### Key logic groups in `CardViewLogic`

- `Guard` — skip refresh, drag visual, feedback when presenting
- `Layout` — scale/collider/highlight math (`ResolvedScale` struct)
- `DragShadow` / `DragVisual` — shadow resolve, show/reset state
- `SelectionHighlight` — highlight resolve and configure
- `SpriteRendererOps` — null-safe renderer mutations
- `ChildRenderer` — child lookup + `ResolveSource` enum

### Host pattern example

```csharp
public void SetSortingOrder(int order)
{
    DisableSortingGroup();
    ApplySortingOrders(new CardViewLogic.Sorting.ApplyValues(order));
}
```

Public API on `CardView` did not change. Callers (`SolitaireDragPresenter`, `SolitaireFeatureRegistration`, etc.) required no updates.

---

## Case study 2: `SolitaireHintLogic`

**Location:** `Assets/_Game/Scripts/Project/SolitaireModule/Hints/`

### Before

- ~554 lines in one `internal static class`
- Deep nested loops in tableau hint discovery

### After — partial files

| File | Lines (approx) | Contents |
|------|----------------|----------|
| `SolitaireHintLogic.cs` | 58 | `Collect`, `AutoComplete` orchestration |
| `SolitaireHintLogic.Validation.cs` | 54 | `InputValidation`, `CycleIndex` |
| `SolitaireHintLogic.Shared.cs` | 156 | `MoveFactory`, `HintCollection`, `Execution`, shared guards |
| `SolitaireHintLogic.Foundation.cs` | 94 | Foundation hint scan/append |
| `SolitaireHintLogic.Waste.cs` | 90 | Waste → tableau hints |
| `SolitaireHintLogic.Tableau.cs` | 228 | Tableau moves + `MoveCandidate` struct |
| `SolitaireHintLogic.Stock.cs` | 64 | Stock draw / waste recycle |

### Refactor techniques used

- `HintResults.Fail(out hint)` — single failure path instead of repeated `hint = None; return false`
- `MoveCandidate` struct — bundles `cardId`, `sourceIndex`, `revealsHiddenCard` for tableau scans
- Private scan helpers: `TryResolveCandidate`, `AppendForCandidate`, `TryFindTargetForCandidate`
- `SolitaireHintService.cs` unchanged — still calls `SolitaireHintLogic.Collect.GatherAll(...)`

### Hint priority (unchanged)

1. Foundation moves
2. Reveal tableau moves
3. Waste → tableau
4. Normal tableau moves
5. Stock action

---

## Case study 3: `SolitaireBoardLayoutCalculator`

**Location:** `Assets/_Game/Scripts/Project/SolitaireModule/Layout/`

### Before

- ~581 lines in one `public static class` with many nested internals

### After — partial files

| File | Lines (approx) | Contents |
|------|----------------|----------|
| `SolitaireBoardLayoutCalculator.cs` | 50 | `SolitaireBoardLayoutResult` + public API |
| `SolitaireBoardLayoutCalculator.Core.cs` | 98 | `Validation`, `Frustum`, `Orientation`, `Viewport` |
| `SolitaireBoardLayoutCalculator.Constants.cs` | 29 | Layout constants |
| `SolitaireBoardLayoutCalculator.Sizing.cs` | 131 | `CardSizing`, `RowLayout`, `CardLayoutMetrics` |
| `SolitaireBoardLayoutCalculator.Portrait.cs` | 120 | Portrait rows (`RowHeights`, `RowStarts`) |
| `SolitaireBoardLayoutCalculator.Landscape.cs` | 182 | Landscape top row + overlap (`TopRow`) |
| `SolitaireBoardLayoutCalculator.ResultFactory.cs` | 59 | Config/default result building |

### Shared struct: `CardLayoutMetrics`

Portrait and landscape both need `cardSize`, `cardWidth`, `gap`, `cardScale`. The struct removes duplicated extraction in `Calculate(...)`.

### Public API preserved

- `CreateFromConfig`
- `TryCalculateResponsive`
- `GetLayoutFrustum`
- `GetCenteredRowStartX`

Consumer `SolitaireResponsiveBoardLayout` required no changes.

---

## Step-by-step refactor checklist

1. **Inspect** existing host, prefabs, and callers. Do not change public API unless necessary.
2. **Identify** pure blocks: math, guards, validation, mapping, collection loops.
3. **Create** `internal static class <Name>Logic` (or `partial` root).
4. **Extract** guards first (`Should*`, `Can*`, `Try*`).
5. **Extract** factories/mappers that build structs or value types.
6. **Leave** Unity mutations in host; optionally add `*Ops` appliers for repeated null-safe sets.
7. **Split** into `partial` files when a logic file exceeds ~200 lines or mixes unrelated domains.
8. **Verify** no new managers, no scene search in logic, no duplicate systems.
9. **Validate** in Editor: affected feature paths (drag, hint, responsive layout, etc.).

---

## Partial class rules (C#)

- All parts must use the same namespace and accessibility (`internal static partial class SolitaireHintLogic`).
- Nested classes can live in any part file; they merge into one outer class.
- Prefer one domain per partial file (`Foundation`, `Landscape`, `Tableau`) — not one method per file.
- Keep the smallest public facade in the main file name (e.g. `SolitaireBoardLayoutCalculator.cs`).

---

## Unit testing notes

Logic partials are Editor Mode / test friendly when they:

- take inputs as parameters
- return values or `out` structs
- avoid `UnityEngine.Object` dependencies where possible

Good test targets from these refactors:

- `CardViewLogic.Layout.TryResolveUniformScale`
- `SolitaireHintLogic.CycleIndex.Normalize`
- `SolitaireHintLogic.MoveEquality.AreSameMove`
- `SolitaireBoardLayoutCalculator.RowLayout.GetCenteredRowStartX`
- `SolitaireBoardLayoutCalculator.CardSizing.FitToVerticalLimit`

Keep integration tests on the host for anything that creates `GameObject` or reads `Camera.pixelWidth`.

---

## Anti-patterns to avoid

| Anti-pattern | Why |
|--------------|-----|
| Logic class stores `static` scene refs | Hidden global state; breaks tests |
| Logic class named `*Manager` | Implies lifecycle ownership |
| Every private method moved blindly | Host becomes pointless pass-through |
| New prefab/component per 10-line rule | Component explosion; violates LEGO model |
| `FormerlySerializedAs` during rename | Project rule: restore inspector values manually |

---

## Quick reference — current Solitaire logic map

Full folder ownership: [SOLITAIRE_MODULE_FOLDER_STRUCTURE.md](../03_Hierarchy/SOLITAIRE_MODULE_FOLDER_STRUCTURE.md).

```text
Views/Card/
  CardView.cs (+ Setup, Drag, Visuals, Feedback, Editor partials)
  CardViewLogic.cs (+ Layout, Renderer, Drag, SelectionHighlight partials)

Hints/
  SolitaireHintService.cs
  SolitaireHintLogic.cs (+ Validation, Shared, Foundation, Waste, Tableau, Stock)

Layout/
  SolitaireBoardLayoutCalculator.cs (+ Core, Constants, Sizing, Portrait, Landscape, ResultFactory)
  SolitaireRuntimeLayoutMetrics.cs

Rules/
  SolitairePileMoveRules.cs (+ Shared, SourceStart, TargetAccept, CardInput)
  SolitaireMoveHandlerRegistry.cs (+ Dispatch, Validation, Execution)
```

When adding new Solitaire behavior, check this map and the folder structure doc before creating another large monolithic file.
