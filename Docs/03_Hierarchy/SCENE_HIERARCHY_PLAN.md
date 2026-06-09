# Scene Hierarchy Plan

## Main scene

```text
GameScene
├── GameRuntimeRoot
│   ├── EventManager or GameEventBus
│   ├── GameFlowManager
│   ├── SaveManager
│   ├── AnalyticsManager
│   ├── CameraManager
│   ├── CharacterManager
│   └── LevelLoader
│
├── MainCanvas
│   ├── StartPanel
│   ├── HudPanel
│   └── ResultPanel
│
├── CharacterRoot
│   └── PlayerCharacter
│
├── CameraRoot
│   ├── IntroCamera
│   ├── GameplayCamera
│   ├── SuccessCamera
│   └── FailCamera
│
└── LevelRuntimeRoot
    └── LoadedLevelInstance
```

## Loaded level prefab

```text
Level_01
├── LevelReferenceHolder
├── References
│   ├── CharacterSpawnPoint
│   ├── CameraTarget
│   ├── FinishPoint
│   ├── CollectableRoot
│   └── ObstacleRoot
└── Content
    ├── Track
    ├── Obstacles
    └── Collectables
```

## Reference ownership

- CharacterManager references CharacterRoot/PlayerCharacter.
- LevelLoader emits the loaded level root.
- CharacterManager uses the loaded root's LevelReferenceHolder.
- CameraManager uses serialized camera mapping.
- UI views use local serialized bindings.

## Do not

- drag unrelated canvas objects into managers
- let level content reference managers directly
- make UI views reference distant scene branches
- use scene search to repair authored references
