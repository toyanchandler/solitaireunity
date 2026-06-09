# Validation Checklist

Use this checklist before considering a Unity feature done.

## Code

- Compile has no errors.
- Touched pure rules or state helpers have Edit Mode tests when practical.
- Runtime code does not reference `UnityEditor`.
- Runtime code does not use `FindObjectOfType`, `GameObject.Find`, `Transform.Find`, deep child search, or name-parsed pool discovery for production UI.
- Views do not read raw config for copy/color/rule policy when snapshots should provide it.
- Views do not overwrite baked UI settings during render.
- Large `MonoBehaviour` views are split into view, bindings, animator, motion/config, and focused collaborators.
- Static helpers receive explicit dependencies as parameters.

## Scene

- Each UI root/canvas has exactly one view scope.
- Required serialized references are assigned.
- Cross-root serialized references are absent unless explicitly allowed by the architecture doc.
- Child pool arrays are populated in hierarchy sibling order.
- No deprecated host, migration, or temporary debug scripts remain in the active hierarchy.
- Baked UI settings are correct in scene/prefab/builder, not patched at runtime.

## Data

- ScriptableObjects use private serialized fields and get-only public APIs.
- DTOs/definitions do not expose public mutable fields.
- Config assets live under `Assets/Config/`; C# config types live under `Assets/Scripts/Data/`.
- New UI text, colors, motion, and rule policy flow through config and snapshots.
- Renamed serialized fields are migrated in existing assets/scenes.

## Runtime Smoke

Run the actual user flow touched by the change:

1. Enter play mode.
2. Trigger the primary action.
3. Trigger success and failure paths if both exist.
4. Close/retry/restart/cancel if those actions exist.
5. Watch the console after each step.

Expected:

- No missing binding exceptions.
- No compile or lifecycle exceptions.
- UI updates from snapshots.
- Buttons reflect allowed actions.
- Animations complete and cleanup callbacks run.
- Re-entering the flow does not duplicate listeners, tweens, objects, or particles.

## Build

- Preprocess collectors run or arrays are already serialized.
- Player build does not depend on editor scripts to repair scene state.
- Supported validators or smoke checks pass.
- No secrets, absolute local paths, or machine-specific references are committed.

## Review Rejects

Reject the change if it includes:

- Runtime hierarchy search as normal production logic.
- Cross-canvas serialized references.
- Public mutable config fields.
- Silent null paths for required objects.
- God scripts mixing lifecycle, refs, rules, layout, animation, and state.
- Runtime repair of authored UI settings.
- One-off migration scripts left as permanent project code.
