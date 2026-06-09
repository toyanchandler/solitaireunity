# ScriptableObject Rules

The project uses three different ScriptableObject roles. Do not mix them.

## 1. Config ScriptableObjects

Purpose:

- designer-tuned constants
- level tables
- reward tables
- camera tuning
- economy tuning
- animation durations

Rules:

- private serialized fields
- public get-only APIs
- no runtime mutation from views
- no SDK calls
- no game flow orchestration

Example names:

```text
GameConfig
RewardTableConfig
CameraConfig
LevelCatalog
```

## 2. Saveable ScriptableObjects

Purpose:

- persistent player/progression data
- values that survive app restarts
- values saved through Easy Save or another backend

Allowed logic:

- simple guard and clamp
- getters
- named mutation methods
- capture save data
- apply save data
- notify changed

Forbidden logic:

- deciding when to save
- calling analytics
- calling external SDKs directly unless it is a provider explicitly designed for save backend
- unlocking features by itself
- controlling game flow

Example names:

```text
PlayerSaveableData
CurrencySaveableData
UpgradeSaveableData
SettingsSaveableData
```

## 3. Runtime ScriptableObjects

Purpose:

- resettable runtime state/context/channel
- current state for late subscribers
- inspector-selected communication channel
- no GameManager dependency in prefabs

Allowed logic:

- store current value or snapshot
- reset runtime state
- publish Changed event
- simple clamp and guard
- expose current snapshot

Forbidden logic:

- gameplay decisions
- save/load orchestration
- analytics orchestration
- SDK calls
- scene search

Example names:

```text
GameFlowRuntimeState
PlayerRuntimeContext
SelectedTargetRuntimeContext
RewardPreviewRuntimeContext
```

## The key distinction

Event args say: something happened.

Runtime SO says: this is the current state or channel identity.

If a newly spawned prefab needs to know current player HP, it cannot rely only on old DamageTaken events. It needs a current state source.

## Reset requirement

Every Runtime SO must reset to initial state at Play Mode start, scene/session start, or another documented reset point.

The reset must prevent:

- ghost state
- stale event subscribers
- dirty runtime values surviving across play sessions
- old snapshots driving new scenes
