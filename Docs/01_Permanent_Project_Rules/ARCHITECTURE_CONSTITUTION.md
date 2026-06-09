# Handler Hypercasual Architecture Constitution

This document defines the permanent architecture rules for the project template.

## Core purpose

This architecture is a production template for fast hypercasual game creation. It is designed so that a junior developer or an LLM can add common game features without rediscovering infrastructure decisions.

The architecture optimizes for:

- fast playable prototype delivery
- predictable extension points
- low Unity hierarchy noise
- save and analytics consistency
- late subscriber safety for runtime state
- AI-assisted development friendliness
- simple validation

It is not designed to show off a heavy framework during small tasks.

## Core layers

```text
Config ScriptableObjects
    immutable-ish designer tuning

Saveable ScriptableObjects
    persistent player/progression state
    save data capture/apply

Runtime ScriptableObjects
    resettable runtime state/context/channel
    current data for late subscribers

Managers and Systems
    cross-cutting orchestration
    event subscription
    save, analytics, camera, character, flow

Internal static helpers
    rules, mapping, formatting, appliers, factories

MonoBehaviours
    Unity lifecycle
    serialized scene references
    local orchestration

Views
    render state or snapshots
    publish UI intents
```

## Golden rule

ScriptableObjects answer: what is the current data?

Managers, systems, and rules answer: what should happen?

Views answer: how should this be shown?

## Runtime lookup policy

Production runtime code must not rely on scene search as normal logic.

Forbidden in production paths:

- `FindObjectOfType`
- `GameObject.Find`
- `Transform.Find`
- deep hierarchy search to repair missing references
- name parsing to build pools
- name sorting to define gameplay/UI order

Allowed:

- `TryGetComponent` on a known event-provided root, such as a loaded level root
- editor validators and collectors
- one-time tooling outside player runtime
- explicit serialized references

## Event policy

Events are for things that happened.

State is for what is true now.

Do not use only historical events when late subscribers need current state. Use Runtime ScriptableObject context, GameState, or another current state store.

## Save policy

Save logic must be predictable.

- Persistent values live in Saveable SOs or clearly named save data classes.
- SaveManager or save services orchestrate when save/load happens.
- Random gameplay code must not call the save SDK directly.
- Adding a persistent value should have a documented path.

## Analytics policy

Analytics is a cross-cutting listener, not gameplay logic.

- Gameplay publishes events.
- AnalyticsManager listens.
- AnalyticsManager reads current state from Saveable/Runtime state.
- SDK-specific logic stays behind `IAnalyticsService`.
- Adding a new analytics backend should not require gameplay changes.

## Level contract policy

A level prefab/root exposes authored references through `LevelReferenceHolder`.

Systems do not search the loaded level for spawn points, camera targets, collectable roots, or finish points by name.

## Static helper policy

Internal static classes are allowed and encouraged for stateless logic.

Allowed:

- `*Rules`
- `*Mapper`
- `*Applier`
- `*Formatter`
- `*Factory`
- `*Hashes`

Forbidden:

- hidden mutable runtime state
- event subscription
- service locator reads
- scene search
- holding scene object references

## Default Spyke task policy

For onsite tasks, use the simple template:

- explicit serialized references
- EventManager or small EventBus
- Runtime SO contexts where current state matters
- Saveable SOs for persistent data
- internal static helper extraction
- LevelReferenceHolder contract

Do not make the task depend on full DI, baked wiring, or reflection injection.
