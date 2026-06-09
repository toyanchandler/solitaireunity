# Codex Master Brief

You are refactoring a Unity hypercasual production template. Preserve gameplay behavior first. Improve architecture second. Do not introduce a large DI framework unless explicitly requested.

## Goal

Migrate the old manager-based architecture into a predictable, extendable, LLM-friendly production template.

The template must help junior developers and LLM agents know where to put new code:

- Persistent player data goes into Saveable ScriptableObject data.
- Runtime session state goes into resettable Runtime ScriptableObject contexts or plain runtime state owned by managers/systems.
- Save/load orchestration stays in SaveManager or Save services.
- Analytics orchestration stays in AnalyticsManager or Analytics services.
- Level-specific authored references stay in LevelReferenceHolder.
- Camera switching stays in CameraManager with a small state-to-camera mapping layer.
- Character spawn/position logic consumes LevelReferenceHolder. It must not deep-search the scene.
- MonoBehaviours own Unity lifecycle and serialized references.
- Internal static classes own pure rules, mappings, factories, and stateless appliers.

## Non-goals

Do not build a full framework during the first migration. Do not add Zenject, Saneject, VContainer, Reflex, or another DI container unless the user asks. Do not add runtime reflection injection as a default path. Do not scan MainCanvas or the full scene at runtime to build dependency graphs.

## Behavior preservation

Refactor in small phases. After each phase, the project should compile and the main user flow should still work:

1. Load data.
2. Load level.
3. Spawn or position character.
4. Start game.
5. Win and fail paths.
6. Save on critical moments.
7. Log analytics on key events.
8. Camera state switches.

## Architecture direction

Use this layered model:

```text
Config ScriptableObjects
    designer-tuned immutable-ish data

Saveable ScriptableObjects
    persistent player/progression data
    capture/apply save data
    no gameplay orchestration

Runtime ScriptableObjects
    resettable current state/context/channel
    optional Changed event
    current snapshot available to late subscribers
    no gameplay orchestration

Managers / Systems
    subscribe to events
    orchestrate save, analytics, camera, character, flow
    call rules and update state

Internal static classes
    pure rules, mapping, formatter, applier, snapshot factory
    no hidden state, no scene search

Views / Controllers
    own Unity lifecycle and local scene references
    render current state or snapshots
    no save or analytics calls directly
```

## Critical migration style

Prefer pragmatic clarity over clever abstraction. Keep the architecture easy for a junior developer or LLM to extend.

When unsure, choose the simplest explicit wiring:

- `[SerializeField]` for scene/local references
- Manager `OnEnable`/`OnDisable` for events
- Runtime SO or Saveable SO for current data
- Static rules for decisions
- Validation with loud warnings or errors

## Immediate red flags to remove

- `FindObjectOfType`, `GameObject.Find`, `Transform.Find` in production gameplay/UI logic
- silent fallback to `Vector3.zero` for required authored references
- `_constantSaveables[0]` or other index-based semantic access
- public mutable fields used only for Inspector exposure
- analytics payloads created with `new { ... }.ToString()` if a structured payload interface exists
- ScriptableObjects that call SDKs, save directly, unlock features, or orchestrate game flow
- managers that call `GetComponent` repeatedly when a required contract can be validated once
