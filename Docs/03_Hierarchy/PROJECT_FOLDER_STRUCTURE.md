# Project Folder Structure

Use a folder layout that shows ownership.

```text
Assets/
├── _Game/
│   ├── Config/
│   │   ├── GameConfig.asset
│   │   ├── AnalyticsConfig.asset
│   │   ├── CameraConfig.asset
│   │   └── LevelCatalog.asset
│   │
│   ├── RuntimeState/
│   │   ├── GameFlowRuntimeState.asset
│   │   ├── PlayerRuntimeContext.asset
│   │   └── RewardPreviewRuntimeContext.asset
│   │
│   ├── SaveData/
│   │   ├── PlayerSaveableData.asset
│   │   ├── CurrencySaveableData.asset
│   │   └── UpgradeSaveableData.asset
│   │
│   ├── Prefabs/
│   │   ├── Levels/
│   │   ├── UI/
│   │   ├── Character/
│   │   └── Cameras/
│   │
│   └── Scenes/
│
└── Scripts/
    ├── Core/
    │   ├── EventManager.cs
    │   ├── GameState.cs
    │   └── GameFlowManager.cs
    │
    ├── Managers/
    │   ├── SaveManager.cs
    │   ├── AnalyticsManager.cs
    │   ├── CameraManager.cs
    │   └── CharacterManager.cs
    │
    ├── Save/
    │   ├── ISaveableProvider.cs
    │   ├── SaveableData.cs
    │   └── Providers/
    │
    ├── RuntimeState/
    │   ├── ResettableRuntimeObject.cs
    │   └── Contexts/
    │
    ├── Level/
    │   ├── LevelReferenceHolder.cs
    │   ├── LevelLoader.cs
    │   └── LevelRules.cs
    │
    ├── Analytics/
    │   ├── IAnalyticsService.cs
    │   ├── DebugAnalyticsService.cs
    │   └── UnityAnalyticsService.cs
    │
    ├── Views/
    │   ├── Hud/
    │   ├── ResultPanel/
    │   └── StartPanel/
    │
    └── Rules/
        ├── CameraStateMapper.cs
        ├── AnalyticsPayloadFactory.cs
        └── CurrencyFormatRules.cs
```

## Folder rules

- Config assets live under `Assets/_Game/Config/`.
- Runtime state assets live under `Assets/_Game/RuntimeState/`.
- Persistent save data assets live under `Assets/_Game/SaveData/`.
- Code defining those assets lives under `Assets/Scripts/`.
- Editor-only tools live under `Assets/Scripts/Editor/`.
