# Validation Checklist

Run this before considering a refactor or feature done.

## Compile

- Project compiles.
- No missing namespaces.
- No runtime assembly references UnityEditor.
- Serialized field renames are migrated or manually verified.

## Runtime lookup

Reject production runtime code using:

- `FindObjectOfType`
- `GameObject.Find`
- `Transform.Find`
- deep child search to repair missing scene references
- name parsing for pools or authored references

Allowed only in editor tooling, validators, or explicit one-time controlled tooling.

## Save

- SaveManager has no semantic array index access like `_constantSaveables[0]`.
- Saveable providers are explicit or grouped clearly.
- Save SDK calls are isolated.
- Adding a new persistent variable has a documented path.
- Save/load works after restart.

## Analytics

- Analytics SDK calls are isolated behind `IAnalyticsService`.
- AnalyticsManager listens to events.
- AnalyticsManager does not mutate gameplay.
- Gameplay and views do not call SDKs directly.
- Payloads are structured if the service supports it.

## ScriptableObjects

- Config SOs are not runtime-mutated by views.
- Saveable SOs contain persistent data and save capture/apply only.
- Runtime SOs reset correctly.
- Runtime SOs do not contain gameplay orchestration.
- SOs do not search scene or call SDKs.

## Level contract

- Every level root has LevelReferenceHolder.
- Required references are assigned.
- CharacterManager uses LevelReferenceHolder.
- Missing spawn point is not silently replaced by Vector3.zero unless explicitly configured.

## Managers

- Managers subscribe in OnEnable and unsubscribe in OnDisable when possible.
- Required refs are validated.
- Managers do not become unrelated god objects.

## Static helpers

- Static helpers are stateless.
- Static helpers receive dependencies as parameters.
- Static helpers do not store scene refs.
- Static helpers do not subscribe to events.

## Runtime smoke

Run:

1. Enter play mode.
2. Load data.
3. Load level.
4. Start level.
5. Trigger success.
6. Trigger fail.
7. Spend currency or trigger save event.
8. Observe analytics debug output.
9. Disable/enable a UI view or runtime subscriber.
10. Exit and re-enter play mode.

Expected:

- no missing binding exceptions
- no null reference from required refs
- save data persists
- runtime SO state resets where expected
- late subscribers render current state
- cameras switch correctly
- character appears at level spawn point
