# Phased Refactor Plan

Refactor in phases. Do not change everything at once. Each phase should compile before moving to the next.

## Phase 0 - Inventory and freeze behavior

1. List all managers.
2. List all static EventManager surfaces.
3. List all Saveable ScriptableObjects.
4. List all Runtime ScriptableObjects.
5. List all LevelReferenceHolder fields.
6. List all analytics events.
7. Search for forbidden runtime lookup APIs.
8. Search for public mutable fields.
9. Search for array index semantics like `_constantSaveables[0]`.

Deliverable:

```text
Docs/Architecture/InventoryReport.md
```

Include:

- current manager responsibilities
- current event surfaces
- current save data types
- current level references
- risky files
- behavior that must be preserved

## Phase 1 - Encapsulation pass

Goal: make fields explicit without changing architecture.

Actions:

- Convert public Inspector fields to `[SerializeField] private` where possible.
- Add get-only properties for data that must be read externally.
- Rename fields to intent-revealing names.
- Add simple `ValidateReferences()` methods to managers with required serialized references.
- Do not change game flow yet.

Acceptance:

- Project compiles.
- Existing prefabs/scenes still have references assigned.
- No serialized data lost after renames.

## Phase 2 - SaveManager safety

Goal: preserve save behavior while removing brittle access.

Actions:

- Replace `_constantSaveables[0]` with explicit `_playerSaveable` or named SaveGroup.
- Make saveable arrays private serialized.
- Add null checks with loud warnings.
- Add optional save order support if existing data has dependencies.
- Keep Easy Save backend unchanged.
- Do not move save SDK calls into ScriptableObjects if they are not already there.

Acceptance:

- LoadAll works on cold start.
- SaveAll works from inspector button.
- Level success saves player data.
- Currency spend triggers save or dirty-save behavior.

## Phase 3 - Analytics service boundary

Goal: analytics SDK can be swapped without touching gameplay.

Actions:

- Keep AnalyticsManager as the single analytics listener.
- Use `IAnalyticsService` interface.
- Add `DebugAnalyticsService` for local/testing.
- Add `CompositeAnalyticsService` if multiple SDKs must receive events.
- Replace anonymous-object `ToString()` payloads with structured payloads if interface supports it.
- AnalyticsManager reads current state from Saveable/Runtime state. Gameplay classes do not call SDKs.

Acceptance:

- GameStart, LevelStart, LevelSuccess, LevelFail, DataLoaded, LevelLoaded still log.
- New analytics backend can be added by implementing service interface.

## Phase 4 - Runtime ScriptableObject reset contract

Goal: runtime state assets cannot leak Play Mode values.

Actions:

- Introduce or standardize `ResettableRuntimeObject`.
- Every Runtime SO implements `ResetRuntimeState()`.
- Runtime SOs may store current value/snapshot and notify `Changed`.
- Runtime SOs must not contain gameplay decisions or SDK calls.
- Add play-mode reset hook or OnEnable reset strategy.

Acceptance:

- Enter Play Mode, mutate Runtime SO, exit Play Mode, re-enter Play Mode. State resets.
- No ghost listeners remain if events are reset.
- Persistent Saveable SO data is not reset like runtime state unless intended.

## Phase 5 - LevelReferenceHolder contract

Goal: levels are authored by contract, not discovered by search.

Actions:

- Add/standardize `LevelReferenceHolder` on every level root.
- Required references include CharacterSpawnPoint at minimum.
- Optional references are explicitly marked as optional.
- CharacterManager consumes LevelReferenceHolder once on level load.
- Remove repeated `GetComponent` calls and silent Vector3.zero fallbacks where possible.

Acceptance:

- Every level prefab/root has LevelReferenceHolder.
- Character spawns or repositions using holder.
- Missing required references produce clear warning or validation error.

## Phase 6 - CameraManager cleanup

Goal: camera transitions are state-driven and easy to extend.

Actions:

- Keep camera refs serialized.
- Private dictionary or list maps game state or camera mode to virtual camera.
- Add missing fail-state event subscription if applicable.
- Prefer `TryGetValue`.
- Warn when no camera exists for requested state.
- Optional: introduce `CameraMode` and `CameraStateMapper` to decouple camera from full game flow enum.

Acceptance:

- Camera changes on GameStarted, LevelStart, Success, Fail if supported, EndMetaStart.
- Unknown state does not hard crash unless configured as required.

## Phase 7 - Internal static helpers

Goal: reduce MonoBehaviour/component count without hiding state.

Actions:

- Extract pure decisions into `internal static *Rules`.
- Extract mapping into `internal static *Mapper`.
- Extract UI/animation assignment into `internal static *Applier` when all refs are explicit parameters.
- Do not make static classes hold runtime references or subscribe to events.

Acceptance:

- MonoBehaviours get smaller.
- Component count does not increase unnecessarily.
- Static classes are stateless and parameter-driven.

## Phase 8 - Documentation and validation

Goal: future LLMs and juniors can extend the project safely.

Actions:

- Add permanent rules under `Assets/Docs/Architecture/` or `Docs/Architecture/`.
- Add how-to docs for save, analytics, level, runtime state, camera, UI.
- Add PR checklist.
- Add Codex validation prompt.

Acceptance:

- A new developer can add a saveable variable by following one doc.
- A new developer can add an analytics event without touching gameplay logic.
- A new level can be created by following LevelReferenceHolder contract.
