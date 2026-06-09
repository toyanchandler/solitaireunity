# How To Work In This Architecture

This guide is for junior developers and LLM agents.

## Before adding code, ask this

### Is it persistent data?

Examples:

- player level
- currency
- upgrade level
- settings
- unlock state

Put it in a Saveable ScriptableObject or save data class.

### Is it current runtime state?

Examples:

- selected target
- current player HP
- current run score
- current level phase
- temporary reward preview

Put it in a Runtime ScriptableObject context or runtime state owned by a manager/system.

### Is it a one-time event?

Examples:

- damage taken
- reward claimed
- button clicked
- level started

Use EventManager or GameEventBus event.

### Is it a decision or calculation?

Use an internal static `*Rules`, `*Mapper`, or `*Factory` class.

### Is it a Unity reference or lifecycle owner?

Use MonoBehaviour with serialized references.

### Is it UI?

UI renders current state or snapshots. UI does not save, analytics-log, or decide gameplay rules.

## Do not do these

- Do not add `FindObjectOfType` to solve dependencies.
- Do not add SDK calls inside gameplay scripts.
- Do not make ScriptableObjects unlock features by themselves.
- Do not add a new MonoBehaviour for every tiny helper.
- Do not use array index to mean a specific saveable.
- Do not silently fallback to zero or null for required authored references.

## Safe extension pattern

```text
Feature event
    -> Manager/System hears it
        -> Rules compute decision
            -> State/Saveable/Runtime SO is updated
                -> UI renders state
                -> Save/Analytics managers react if needed
```
