# Manager Rules

Managers orchestrate cross-cutting systems. They should not become god objects.

## Manager responsibilities

Allowed manager responsibilities:

- subscribe/unsubscribe to project events
- update Runtime SO or Saveable SO state through named methods
- call save/analytics/camera/character services
- coordinate loaded level contracts
- call internal static rules/mappers

Managers should not:

- deep-search scene hierarchy
- own unrelated UI references
- call SDKs outside their boundary
- hold temporary feature logic that belongs in a feature system
- silently ignore required missing references

## Lifecycle

Preferred lifecycle:

```text
Awake
    cache own references
    construct internal services
    validate serialized fields

OnEnable
    subscribe to events

OnDisable
    unsubscribe from events
```

Use `OnDestroy` only when the object is guaranteed not to be disabled/enabled during session or when cleaning unmanaged resources.

## Manager examples

### SaveManager

Orchestrates Saveable providers. Does not own gameplay decisions.

### AnalyticsManager

Listens to gameplay events and logs through `IAnalyticsService`. Does not mutate gameplay.

### CameraManager

Maps game state to camera mode and activates the proper virtual camera.

### CharacterManager

Consumes LevelReferenceHolder and positions/activates the character.

## Required validation

Managers with required serialized fields must validate them. A missing character reference, missing camera entry, or missing saveable provider should produce a clear warning or error.
