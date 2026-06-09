# Save and Analytics Rules

Save and analytics are cross-cutting systems. They must not spread across gameplay classes.

## Save pipeline

Default flow:

```text
Gameplay event happens
    SaveManager hears it
        SaveManager saves relevant Saveable SOs
            Save provider uses Easy Save or backend
```

Saveable data classes should not decide when save happens. They can expose data and accept loaded data.

## Adding persistent values

Adding a new persistent value should be boring:

1. Add the field to the correct Saveable SO.
2. Expose get-only property and named mutation method.
3. Include the field in save data capture/apply.
4. Ensure the Saveable SO is registered in SaveManager or SaveRegistry.
5. Run Save validation.

Do not add ad-hoc save calls inside feature MonoBehaviours.

## Save backend

Easy Save or any other backend should be isolated behind save provider methods or services.

Allowed:

```text
SaveManager
SaveService
SaveableProvider implementation
```

Avoid:

```text
PlayerController
HudView
RewardPopupView
Obstacle scripts
CameraManager
```

## Analytics pipeline

Default flow:

```text
Gameplay event happens
    AnalyticsManager hears it
        AnalyticsManager reads current state if needed
            IAnalyticsService logs event
```

Gameplay code does not call Unity Analytics, GameAnalytics, Firebase, or other SDKs directly.

## Analytics service interface

The project should support replacing or combining analytics SDKs:

```text
IAnalyticsService
    UnityAnalyticsService
    GameAnalyticsService
    DebugAnalyticsService
    CompositeAnalyticsService
```

## Structured payloads

Do not rely on anonymous-object `ToString()` for analytics payloads if the service supports structured parameters.

Prefer:

```csharp
new Dictionary<string, object>
{
    ["level"] = playerSaveableData.LevelIndex
}
```

## Analytics event naming

Use stable event names:

```text
GameStart
DataLoaded
LevelLoaded
LevelStart
LevelSuccess
LevelFail
CurrencySpent
UpgradePurchased
RewardClaimed
```

## Validation

Reject if:

- gameplay class calls analytics SDK directly
- view class calls analytics SDK directly
- random class calls ES3 directly
- save depends on array index semantics
- analytics event lacks a clear source event or state source
