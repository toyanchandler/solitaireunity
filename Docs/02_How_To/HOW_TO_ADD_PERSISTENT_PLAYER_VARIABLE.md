# How To Add A Persistent Player Variable

Use this when adding data that must survive app restarts.

Examples:

- coin amount
- current level index
- upgrade level
- skin unlock state
- no ads purchase state

## Steps

1. Find the correct Saveable SO.

Examples:

```text
PlayerSaveableData
CurrencySaveableData
UpgradeSaveableData
SettingsSaveableData
```

2. Add a private serialized field.

```csharp
[SerializeField] private int _upgradeLevel;
public int UpgradeLevel => _upgradeLevel;
```

3. Add a named mutation method.

```csharp
public void SetUpgradeLevel(int value)
{
    _upgradeLevel = Mathf.Max(0, value);
    Changed?.Invoke();
}
```

4. Add the field to save capture/apply.

```csharp
public PlayerSaveData CaptureSaveData()
{
    return new PlayerSaveData(
        levelIndex: _levelIndex,
        upgradeLevel: _upgradeLevel
    );
}

public void ApplySaveData(PlayerSaveData data)
{
    _levelIndex = data.LevelIndex;
    _upgradeLevel = data.UpgradeLevel;
    Changed?.Invoke();
}
```

5. Confirm the Saveable SO is registered in SaveManager or SaveRegistry.

6. Decide when it should save.

Use existing SaveManager triggers if possible:

- level success
- currency spent
- upgrade purchased
- app pause
- app quit

7. Add analytics only in AnalyticsManager if needed.

Do not log analytics from the Saveable SO.

## Validation

- Value loads after app restart.
- Value saves after relevant trigger.
- No direct ES3 call was added to gameplay/view class.
- No public mutable field was added.
