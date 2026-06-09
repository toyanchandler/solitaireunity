# Inventory Report

Generated before the Codex refactor implementation. Scope is first-party code under `Assets/_Game`.

## Rule Sources Read

- `Docs/01_Permanent_Project_Rules`: architecture, manager, save/analytics, ScriptableObject, static helper, baked wiring rules.
- `Docs/04_Validation`: hard rejects, validation checklist, PR checklist, Codex validation criteria.
- `Docs/99_Source_Baseline`: source architecture, hierarchy wiring, advanced binding, validation baseline.
- `Docs/00_Codex_Refactor`: master brief, execution prompt, phased plan, validation prompt.

## Managers And Core Services

- Ads: `AdsManager`, `UnityAdsService`.
- Analytics: `AnalyticsManager`, `UnityAnalyticsService`, `IAnalyticsService`.
- Audio: `AudioManager`, `CrossFadeAudioService`.
- Camera: `CameraManager`.
- Character: `CharacterManager`, `CharacterAnimationController`, `EntityAnimationProvider`.
- Game state: `GameStartHandler`, `GameStateData`, `GameStateManager`.
- Haptics: `HapticManager`.
- Level: `LevelManager`, `ActiveLevelTimerProvider`, `LevelReferenceHolder`.
- Save: `SaveManager`.
- Event surface: partial `EventManager` classes under `Managers/Core/EventManagers`.

## Event Surface

- In-game: `GameStarted`, `LoadLevel`, `BeforeLevelLoaded`, `LevelLoaded`, `LevelStart`, `LevelSuccess`, `EndMetaStart`, `LevelRestart`, `LevelFail`.
- Save: `DataSaved`, `DataLoaded`.
- Audio: `AudioAdded`, `AudioStop`, `AudioPlay`, `VolumeChange`, `AudioChanged`, `AudioLoopToggleChanged`, `AudioEnabled`.
- Ads: `RewardedShow`, `InterstitialReward`.
- Gameplay systems: `Clickable`, `Collectable`, `Health`, `Interactable`, `Obstacle`, `Path`, `Shootable`, `Stackable`, `Timer`, `UpgradeSystem`, `CurrencySystem`.

## ScriptableObject Inventory

- Saveable/persistent: `PersistentSaveManager<T>`, `PlayerSaveableData`, `SettingsDataSO`, `CollectableValuesSO`, `DeathValueSO`, `LevelTimerValuesSO`, `StackDataSO`.
- Runtime/reset: `IResettable`, `ResettableScriptableObject`, `ResettableData`, `ResetManager`.
- Config/predefined: `LevelList_SO`, `Level_SO`, `IconProviderSO`, `UIAnimationPrefabSO`.

## Level Contract

- Current `LevelReferenceHolder` exposes `SuccessTrigger`, `CharSpawnPoint`, and optional camera, collectable, obstacle, tutorial, and bounds roots.
- Existing level prefabs with holder data include `LevelMaster` and `Level_1`; spline-specific level prefabs were removed.
- The documented target contract requires `CharSpawnPoint` and supports optional camera, finish, collectable, obstacle, tutorial, and bounds roots.

## Current Risks To Remove

- `SaveManager` uses semantic array access: `_constantSaveables[0]`.
- `AnalyticsManager` uses anonymous-object `.ToString()` payloads and unsubscribes in `OnDestroy`.
- `CharacterManager` calls `GetComponent<LevelReferenceHolder>()` repeatedly and silently falls back to `Vector3.zero` when spawn is missing.
- `CameraManager` has a fail handler but does not subscribe to `LevelFail`.
- First-party public mutable Inspector fields are common in managers, UI, input, interactable, combat, upgrade, runtime data, and saveable data.
- `GetComponentsInChildren` appears in interactable/clickable/damageable action collection; this is local same-root action discovery and should be reviewed separately from scene-wide dependency search.
- `ES3` calls are isolated to editor level tooling, `PersistentSaveManager<T>`, and `SaveManager` editor clear button.

## Behavior To Preserve

- Data load on startup.
- Level instantiation from level list or holder fallback.
- Character activation before level load and spawn on level load.
- Game start, level start, success, fail, and end-meta events.
- Save on level success and collectable spend.
- Analytics events: `GameStart`, `LevelStart`, `LevelSuccess`, `LevelFail`, `DataLoaded`, `LevelLoaded`.
- Camera switches for loaded/start/success/fail/end-meta states.
