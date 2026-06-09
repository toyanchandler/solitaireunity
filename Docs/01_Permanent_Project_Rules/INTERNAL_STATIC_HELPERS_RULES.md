# Internal Static Helpers Rules

Internal static classes are allowed. They are useful for keeping Unity hierarchy clean and avoiding unnecessary component explosion.

## Why use them

Not every logic split deserves a MonoBehaviour.

A helper MonoBehaviour creates costs:

- a component must exist on prefab/scene
- references must be assigned
- lifecycle order matters
- junior developers may forget to add it
- LLMs may create too many components

An internal static helper can separate logic without adding scene wiring.

## Allowed static classes

```text
PlayerMovementRules
PlayerAnimationApplier
CameraStateMapper
AnalyticsPayloadFactory
CurrencyFormatRules
SaveKeyBuilder
LevelSpawnRules
RewardCalculationRules
HudSnapshotFactory
```

## Naming

Prefer names that describe behavior:

- `*Rules` for decisions
- `*Mapper` for mapping one domain to another
- `*Applier` for applying data to explicit refs
- `*Factory` for building DTOs or snapshots
- `*Formatter` for text formatting
- `*Hashes` for cached Animator hashes

Avoid naming a static helper `*Controller` unless it truly controls nothing and the team accepts the naming. `Controller` often implies lifecycle ownership.

## Required constraints

Static helpers must:

- be stateless
- receive dependencies as parameters
- not read service locators
- not search the scene
- not subscribe to events
- not hold scene refs
- not mutate hidden global state

## Good example

```csharp
internal static class CameraStateMapper
{
    public static CameraMode ToCameraMode(GameState state)
    {
        return state switch
        {
            GameState.LevelLoaded => CameraMode.Intro,
            GameState.LevelStart => CameraMode.Gameplay,
            GameState.LevelEnd => CameraMode.Success,
            GameState.Fail => CameraMode.Fail,
            GameState.EndMetaStart => CameraMode.Meta,
            _ => CameraMode.Gameplay
        };
    }
}
```

## Good applier example

```csharp
internal static class PlayerAnimationApplier
{
    public static void Apply(Animator animator, PlayerAnimationPose pose, float speed)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetInteger(PlayerAnimationHashes.Pose, (int)pose);
        animator.SetFloat(PlayerAnimationHashes.Speed, speed);
    }
}
```

## Bad example

```csharp
internal static class PlayerAnimationController
{
    private static Animator _animator;

    public static void Initialize()
    {
        _animator = Object.FindObjectOfType<Animator>();
    }
}
```

This is not a helper. It is hidden global state and scene search.

## Large file split pattern

When a `*Logic` class or static calculator grows past ~200 lines, split it with C# `partial` files and keep the host thin.

Worked examples (CardView, hint engine, board layout):

- [HOW_TO_REFACTOR_WITH_LOGIC_PARTIALS.md](../02_How_To/HOW_TO_REFACTOR_WITH_LOGIC_PARTIALS.md)
