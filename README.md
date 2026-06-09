# Klondike Solitaire BaseProject

A classic Klondike Solitaire module built inside the Handler Unity BaseProject template. This is not a greenfield Unity sample: the game is composed through the existing LEGO-style, component-driven, inspector-driven, event-based project architecture.

The implementation follows the reference shape from [XeldarAlz/KlondikeSolitaire](https://github.com/XeldarAlz/KlondikeSolitaire) while staying native to this project: no new global managers, no duplicate event bus, no replacement input stack, and no scene rebuild.

> Current quality gate: **20 automated tests** across EditMode and PlayMode, plus repeatable editor benchmark tooling and scene/prefab validation tooling.

## About

This repository is a playable Solitaire-focused BaseProject variant and a template example for composing a full card game with the project architecture:

- **Clean Klondike rules** - draw-1 stock, waste, 7 tableau columns, 4 foundations, alternating-color tableau builds, same-suit ascending foundations.
- **Scene-first module wiring** - cards, slots, board camera, drag layer, and controllers self-register through the existing Solitaire event channel.
- **Undo-ready runtime state** - move history stores board snapshots and restores pile/card face state.
- **Score display** - score is event-driven through `EventManager.SolitaireEvents` and configured by `SolitaireScoreConfigSO`.
- **Hint support** - legal move hints are collected through the existing rules/move services and shown through a focused presenter.
- **AutoComplete support** - deterministic foundation moves execute through the existing move path, preserving score and undo behavior.
- **Responsive portrait board and HUD** - board layout is recalculated from runtime state and the HUD switches between a two-row portrait layout and a right-side landscape stack.
- **Debug scenarios** - editor/play-mode scenario tooling exists for targeted QA setups.
- **No menu/ad layer in the module** - Solitaire reports success through the base project flow instead of owning global UI.

## Performance Budget

| Metric | Budget / Current Contract |
| --- | --- |
| Runtime card instances | 52 authored/reused card views |
| Normal restart allocation | No runtime card Instantiate/Destroy path |
| Card lookup | Fixed 52-slot registry |
| Layout recalculation | Bounded by 52 cards |
| Move validation | Rules-side validation with no scene search |
| Draw calls | Keep card sprites atlas/batching friendly |

### Local Benchmarks

Latest measured run:

- Unity `6000.3.16f1`
- Platform `OSXEditor`
- Device `Apple M3 / Apple M3`
- Runner: `Tools/Solitaire/Run Benchmarks`

| Benchmark | Iterations | Total | Average | Throughput |
| --- | ---: | ---: | ---: | ---: |
| Initial deal | 20,000 | 129.230 ms | 6.461 us | 154,763 ops/s |
| Tableau move validation | 250,000 | 26.371 ms | 0.105 us | 9,480,290 ops/s |
| Stock draw plus undo | 20,000 | 104.574 ms | 5.229 us | 191,253 ops/s |
| Snapshot create plus restore | 20,000 | 80.017 ms | 4.001 us | 249,948 ops/s |
| Score event dispatch plus update | 500,000 | 3.463 ms | 0.007 us | 144,366,807 ops/s |
| Hint enumeration | 100,000 | 268.426 ms | 2.684 us | 372,542 ops/s |
| AutoComplete foundation sweep | 20,000 | 818.944 ms | 40.947 us | 24,422 ops/s |

Runtime rendering snapshot from the current editor session captured `31` draw calls, `31` batches, `17` set-pass calls, `361` triangles, and `935` vertices during Play Mode warm-up. A stable FPS number is intentionally not listed yet because the editor session did not remain in Play Mode long enough for a steady-state capture; do not replace this with a guessed FPS value.

HUD layout was revised after Hint/AutoComplete were added: 390x844 portrait and 844x390 landscape bounding-box checks for Moves, Score, Undo, Hint, and AutoComplete returned no overlaps. Real gameplay screenshots are still kept as a deployment validation step rather than claimed here.

### Rendering Optimizations

- **Pre-authored card views** - the board reuses existing card objects instead of spawning during normal play.
- **Fixed registry** - `SolitaireViewRegistry` maps card ids directly to `CardView` instances.
- **Responsive sizing** - `SolitaireLayoutController` scales slots/cards from config and board camera metrics.
- **Layered drag presentation** - dragged cards move under `DragParent` and render above board cards.
- **Config-driven visuals** - `SolitaireCardVisualCatalogSO` owns card sprite lookup instead of hardcoded runtime choices.

## Architecture

```text
Data/          ScriptableObject config and shared Solitaire enums
Rules/         Pure card ids, move resolution, validation helpers
Runtime/       Board state, snapshots, move history, feature registration
Input/         Board hit testing, pointer sampling, drop target resolution
Presentation/  Drag presentation helpers
Views/         Card, slot, drag layer, and visual state components
Controllers/   Deck, layout, input, camera, haptics, flow bridges
Editor/        Scene repair, validation, and debug scenario tooling
Tests/         EditMode rules/runtime tests and PlayMode wiring smoke tests
```

- Uses the existing `EventManager.SolitaireEvents` partial, not a new event bus.
- Uses `SolitaireModuleBootstrap` as the composition root; it owns config only.
- Uses `SolitaireFeatureRegistration` as a session registry for self-registered scene objects.
- Uses `SolitaireMoveResolver` for validation and `SolitaireMoveExecutor` for board mutation.
- Uses `ScoreTextManager` plus `SolitaireScoreConfigSO` for score presentation without coupling UI to controllers.
- Uses `SolitaireHintService` for hint enumeration and autocomplete eligibility without creating a separate rules system.
- Uses existing Unity Test Framework assemblies for EditMode and PlayMode coverage.
- Avoids `FormerlySerializedAs` and replacement manager architecture.

## Tech Stack

Unity 6, URP, Unity UI, DOTween, NiceVibrations, Unity Test Framework, MCP for Unity, BaseProject `EventManager`, ScriptableObject config assets.

## Testing

20 tests are provided under `Assets/_Game/Tests/SolitaireModule`:

- **EditMode (16):** initial deal, 52-card uniqueness, tableau legality, foundation legality, waste top-card rule, stock draw, waste recycle, undo, invalid no-mutation guard, win detection, score config/event guards, hint ordering/cycling, autocomplete move path, module layer/serialization guards.
- **PlayMode (4):** controller host self-registration, drag layer registration, board camera registration/screen conversion, duplicate slot fail-fast behavior.

Recommended local commands:

```bash
/Users/bengisucay/Unity/Hub/Editor/6000.3.16f1/Unity.app/Contents/MacOS/Unity \
  -projectPath /Users/bengisucay/Unity/BaseProject -batchmode -nographics \
  -executeMethod _Game.Scripts.Project.SolitaireModule.Editor.SolitaireTestRunnerUtility.RunEditModeAndExit \
  -logFile /tmp/baseproject-test-results/api-editmode.log

/Users/bengisucay/Unity/Hub/Editor/6000.3.16f1/Unity.app/Contents/MacOS/Unity \
  -projectPath /Users/bengisucay/Unity/BaseProject -batchmode -nographics \
  -executeMethod _Game.Scripts.Project.SolitaireModule.Editor.SolitaireTestRunnerUtility.RunPlayModeAndExit \
  -logFile /tmp/baseproject-test-results/api-playmode.log
```

Latest local evidence: EditMode `16/16` passed in `/tmp/baseproject-test-results/api-editmode.xml`; PlayMode `4/4` passed in `/tmp/baseproject-test-results/api-playmode.xml`.

Editor validation is also available through:

- `Tools/Solitaire/Validate Main Scene`
- `Tools/Solitaire/Repair Main Scene`
- `Tools/Solitaire/Run Benchmarks`
- `SolitaireModuleBootstrap.Validate`

## How to Play

- Drag or click a card to move it.
- Build tableau columns downward in alternating colors.
- Build foundations upward by suit, Ace to King.
- Tap stock to draw one card to waste.
- Tap empty stock to recycle waste when recycle is enabled.
- Use Undo to restore the previous move state.
- Use Hint to cycle through currently legal move suggestions.
- Use AutoComplete when safe foundation moves are available.
- Win by placing all 52 cards onto the foundations.

## Documentation

| Document | Description |
| --- | --- |
| [Docs/SolitaireKlondike_GDD.md](Docs/SolitaireKlondike_GDD.md) | Game design, rules, acceptance criteria, QA scenarios |
| [Docs/03_Hierarchy/SOLITAIRE_MODULE_RUNTIME_WIRING.md](Docs/03_Hierarchy/SOLITAIRE_MODULE_RUNTIME_WIRING.md) | Runtime registration and scene wiring contract |
| [Docs/README_TR.md](Docs/README_TR.md) | BaseProject architecture migration overview |
| [Docs/04_Validation/VALIDATION_CHECKLIST.md](Docs/04_Validation/VALIDATION_CHECKLIST.md) | Project validation checklist |

## About the Workflow

This module is assembled to match BaseProject rules first: existing components, inspector references, ScriptableObject config, runtime context, EventManager events, and focused adapter components. Frameworks from the external reference are mapped to the local architecture instead of imported as parallel systems.
