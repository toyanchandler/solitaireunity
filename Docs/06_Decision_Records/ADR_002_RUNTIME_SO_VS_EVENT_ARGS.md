# ADR 002 - Runtime ScriptableObject vs Event Args

## Decision

Use event args for one-time events. Use Runtime ScriptableObjects for current state/context that late subscribers need.

## Why

Event args represent history:

```text
DamageTaken happened.
HealthApplied happened.
```

A newly spawned prefab missed those events and cannot know current health from history unless replay/state exists.

Runtime SO represents current state or channel identity:

```text
PlayerRuntimeState.CurrentHealth = 30
LowHealthFeatureState.IsUnlocked = true
```

A newly spawned prefab can read current state immediately.

## Rules

Use event args when:

- the event is one-time
- late subscriber does not need old value
- payload is only relevant at that moment

Use Runtime SO when:

- current value matters
- late subscribers must render current state
- prefab should not depend on GameManager
- channel identity should be assigned in Inspector

## Constraint

Runtime SO must not become gameplay orchestration. Logic stays in systems and rules.
