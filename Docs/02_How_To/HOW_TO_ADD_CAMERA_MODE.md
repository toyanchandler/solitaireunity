# How To Add A Camera Mode

Camera switching should be state-driven.

## Steps

1. Add a new camera mode if needed.

```csharp
public enum CameraMode
{
    Intro,
    Gameplay,
    Success,
    Fail,
    Meta,
    Bonus
}
```

2. Add a virtual camera to the scene/prefab.

3. Add the camera to CameraManager's serialized mapping.

4. Update `CameraStateMapper` if mapping from GameState.

```csharp
GameState.BonusStart => CameraMode.Bonus
```

5. Ensure the event that changes state is published.

## Do not

- let gameplay scripts directly activate virtual cameras
- search cameras by name
- assume every GameState has a camera without validation

## Validation

- State event switches to correct camera.
- Missing camera mapping produces a clear warning.
- Fail and success paths both switch if supported.
