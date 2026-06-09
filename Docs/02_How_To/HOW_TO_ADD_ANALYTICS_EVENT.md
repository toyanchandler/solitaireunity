# How To Add An Analytics Event

Analytics must be added without touching SDKs in gameplay classes.

## Steps

1. Identify the gameplay event that already represents the moment.

Examples:

```text
LevelStart
LevelSuccess
LevelFail
CurrencySpent
RewardClaimed
UpgradePurchased
```

2. If no event exists, add one to EventManager or GameEventBus.

3. Subscribe in AnalyticsManager.

```csharp
EventManager.InGameEvents.LevelSuccess += LogLevelSuccess;
```

4. Build the payload in AnalyticsManager or `AnalyticsPayloadFactory`.

```csharp
private void LogLevelSuccess()
{
    LogEvent("LevelSuccess", new Dictionary<string, object>
    {
        ["level"] = _playerSaveableData.LevelIndex
    });
}
```

5. Use `IAnalyticsService`.

Do not call Unity Analytics, GameAnalytics, Firebase, or another SDK directly from gameplay.

## Adding a new analytics SDK

1. Implement `IAnalyticsService`.
2. Add it to `CompositeAnalyticsService` or configure AnalyticsManager.
3. Do not change gameplay code.

## Validation

- Event logs at the correct moment.
- Payload uses current state source.
- No gameplay/view class calls analytics SDK.
- AnalyticsManager does not mutate gameplay state.
