# How To Add A New Level

Every level must expose authored references through LevelReferenceHolder.

## Required hierarchy

```text
Level_XX
├── LevelReferenceHolder
├── References
│   ├── CharacterSpawnPoint
│   ├── CameraTarget
│   ├── FinishPoint
│   ├── CollectableRoot
│   └── ObstacleRoot
└── LevelContent
```

## Steps

1. Create the level root.
2. Add `LevelReferenceHolder` to the root.
3. Create reference transforms under `References`.
4. Assign required references in the holder.
5. Add level to LevelCatalog or LevelLoader config.
6. Run level validation.
7. Enter play mode and load the level.

## Required references

Required unless the game mode explicitly says otherwise:

- CharacterSpawnPoint
- FinishPoint

Optional but common:

- CameraTarget
- CollectableRoot
- ObstacleRoot
- TutorialRoot
- BoundsRoot

## Do not

- make CharacterManager search for spawn point by name
- fallback to Vector3.zero silently
- place required references outside the level root
- use scene-wide singleton references inside level content

## Validation

- Level loads.
- Character appears at CharacterSpawnPoint.
- Camera can use CameraTarget if relevant.
- Missing required reference is reported before shipping.
