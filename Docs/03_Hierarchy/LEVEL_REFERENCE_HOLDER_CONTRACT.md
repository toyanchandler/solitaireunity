# LevelReferenceHolder Contract

`LevelReferenceHolder` is the authored contract between level prefabs and runtime managers.

## Purpose

A level should not require managers to search its hierarchy. The level root exposes needed references explicitly.

## Required fields

- `CharacterSpawnPoint`
- `FinishPoint` if the game has a finish condition

## Optional fields

- `CameraTarget`
- `CollectableRoot`
- `ObstacleRoot`
- `TutorialRoot`
- `BoundsRoot`
- `BonusRoot`

Optional fields must be supported variants. They are not optional because someone forgot to wire them.

## Example API

```csharp
public sealed class LevelReferenceHolder : MonoBehaviour
{
    [SerializeField] private Transform _charSpawnPoint;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Transform _finishPoint;
    [SerializeField] private Transform _collectableRoot;
    [SerializeField] private Transform _obstacleRoot;

    public Transform CharSpawnPoint => _charSpawnPoint;
    public Transform CameraTarget => _cameraTarget;
    public Transform FinishPoint => _finishPoint;
    public Transform CollectableRoot => _collectableRoot;
    public Transform ObstacleRoot => _obstacleRoot;

    public bool HasCameraTarget => _cameraTarget != null;

    public bool Validate(out string error)
    {
        if (_charSpawnPoint == null)
        {
            error = $"{name} is missing CharacterSpawnPoint.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
```

## Manager usage

```csharp
if (!levelRoot.TryGetComponent(out LevelReferenceHolder refs))
{
    Debug.LogWarning($"Loaded level {levelRoot.name} has no LevelReferenceHolder.");
    return;
}

if (refs.CharSpawnPoint == null)
{
    Debug.LogWarning($"Loaded level {levelRoot.name} has no CharacterSpawnPoint.");
    return;
}

_character.transform.SetPositionAndRotation(
    refs.CharSpawnPoint.position,
    refs.CharSpawnPoint.rotation
);
```

## Validation

Every level root included in the build or catalog should have a valid holder.
