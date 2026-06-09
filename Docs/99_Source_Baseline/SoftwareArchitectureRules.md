# Software Architecture Rules

This document is the single source of truth for how a Unity project structures runtime code, scene hierarchy, data, events, UI, and editor tooling. If a change conflicts with a rule here, the change is wrong unless this document is updated first.

Related documents:

- `AdvancedSoftwareRules.md` - binding lifecycle, marker components, aggregate bindings, static helpers, and editor tooling.
- `HierarchyWiringRules.md` - scene serialization and cross-system wiring.
- `ValidationChecklist.md` - practical verification before a feature is considered done.

---

## 1. Principles

1. **Data drives presentation** - gameplay, tuning, copy, colors, and motion policies live in ScriptableObjects, definitions, and pure rules. UI renders snapshots.
2. **Thin views** - `MonoBehaviour` views do not contain game rules, copy policy, cross-scope wiring, or large orchestration blocks.
3. **Editor collects, runtime reads** - large child pools and authored references are filled in editor tooling. Play mode does not search the hierarchy to repair missing wiring.
4. **Encapsulation everywhere** - no public mutable fields on config DTOs or ScriptableObject roots. Expose get-only APIs and named mutation methods.
5. **Explicit boundaries** - cross-subtree communication uses a composition root, event bus, same-scope injection, or shared ScriptableObjects only.
6. **One composition root** - a single runtime root bootstraps the session. Do not scatter gameplay graphs through `FindObjectOfType` calls.
7. **Fail loud** - missing required wiring throws a clear exception during bind/setup. Silent null behavior is a bug unless the missing object is a supported variant.
8. **Baked UI stays authored** - runtime may animate, show/hide, and apply snapshot data, but must not overwrite authored settings such as `maskable`, raycast flags, image type, anchors, pivots, TMP wrapping, or static chrome geometry.

---

## 2. Layer Map

Use a folder layout that makes ownership obvious:

```text
Assets/Config/              -> ScriptableObject assets owned by designers
Assets/Scripts/Collections/ -> Runtime metadata and editor collectors for serialized child pools
Assets/Scripts/Data/        -> Definitions, rules, ScriptableObject types, scene data contracts
Assets/Scripts/Diagnostics/ -> Debug/performance monitoring only
Assets/Scripts/Editor/      -> Menus, inspectors, validators, scene builders, asset tools
Assets/Scripts/Runtime/     -> Play-mode gameplay, bootstrap, events, publishing, flow
Assets/Scripts/Views/       -> Scene UI components and presentation collaborators
```

### Runtime folders

| Folder | Responsibility |
| --- | --- |
| `Collections/` | Runtime attributes/metadata only. No editor logic in player assemblies. |
| `Collections/Editor/` | Collect/reset/preprocess/property drawer tooling. |
| `Data/Definitions/` | Serializable DTOs, enums, profiles, and content contracts. |
| `Data/Rules/` | Pure deterministic decision helpers. No hidden random, scene, locator, or Unity object access. |
| `Data/SceneSettings/` | Scene-authored data contracts when a scene owns specific configuration. |
| `Data/ScriptableObjects/` | ScriptableObject classes whose assets live under `Assets/Config/`. |
| `Diagnostics/` | Development overlays, counters, smoke-check helpers, and metric snapshots. |
| `Runtime/Bootstrap/` | Composition root, config source, runtime locator if used. |
| `Runtime/Events/` | Event bus and intent/snapshot event surface. |
| `Runtime/Flow/` | High-level feature/session flow controllers. |
| `Runtime/Game/` | Mutable session state and gameplay result objects. |
| `Runtime/Publishing/` | Immutable snapshots and publishers. |
| `Views/` | Scene-authored UI components, bindings, animation presenters, and panel collaborators. |

Editor code must never become a second runtime. Runtime code must never depend on editor scripts to repair state at launch.

---

## 3. Config and Data

### 3.1 Assets vs types

| Location | Role |
| --- | --- |
| `Assets/Config/*.asset` | Designer-owned Unity asset instances. |
| `Assets/Scripts/Data/` | C# types that describe those assets. |

Never treat `Scripts/Data` as the asset folder.

### 3.2 ScriptableObject roots

ScriptableObject roots expose private serialized fields and public get-only APIs:

```csharp
[SerializeField] private int _itemCount = 8;
public int ItemCount => Mathf.Max(1, _itemCount);
```

Rules:

- Private `[SerializeField]` backing fields.
- Public get-only properties.
- Validation/clamping in getters or explicit validation methods.
- Lists and nested DTOs are not mutated by random views at runtime.

### 3.3 Definitions and DTOs

Definitions follow the same rule:

```csharp
[SerializeField] private Color _primaryColor;
public Color PrimaryColor => _primaryColor;
```

Rules:

- No public fields on definitions.
- No public setters exposed to gameplay.
- Runtime mutation uses named methods that explain intent.
- Editor/bootstrap mutation uses factories or narrow ScriptableObject APIs.

### 3.4 Defaults vs tuning

| Mechanism | Purpose |
| --- | --- |
| `Default()` / `Create(...)` factories | First-time asset creation and reset buttons. |
| `.asset` files in `Assets/Config/` | Source of truth at runtime. |
| Snapshot factory/publisher | Read-only UI-facing projection. |

Code defaults are bootstrap, not hidden runtime magic. Play mode should read configured assets and snapshots, not re-decide designer tuning.

---

## 4. Scene Hierarchy and SerializeField

### 4.1 Allowed references

| From | May reference |
| --- | --- |
| View/component on GameObject `X` | Components on `X` or descendants of `X`. |
| Runtime/UI script | Shared ScriptableObjects under `Assets/Config/`. |
| Composition root | Same-GameObject runtime components and explicit config source. |

### 4.2 Forbidden references

- Dragging widgets from another canvas/root into a local view.
- Serializing a distant branch just because it is convenient.
- Mega inspector lists on the runtime root.
- Runtime `Transform.Find`, `GameObject.Find`, deep child search, or name parsing for production UI pools.

### 4.3 View scopes

Each UI root/canvas owns one view scope. The scope:

1. Builds a same-scope component registry from its own subtree.
2. Injects only same-scope dependencies and runtime services explicitly exposed by the runtime root.
3. Calls lifecycle callbacks on bindable components.
4. Unbinds cleanly on disable/runtime stop.

Do not use legacy central `*UiHost` objects that hold references across unrelated canvases.

### 4.4 Composition root

The runtime root should be small and explicit:

- One composition root component.
- One config-source component if needed.
- Session state, publisher, event bus, and flow controllers on the same root or intentionally owned children.
- Zero large serialized UI lists.

The composition root bootstraps the session; it does not own every scene reference.

---

## 5. View and Binding Split

Large authored UI surfaces should split scene references from runtime orchestration.

| Class shape | Responsibility |
| --- | --- |
| `*View` | Lifecycle, event subscription, snapshot orchestration, public UI intents. |
| `*Bindings` | Serialized authored references, validation, listener hookup, home-state capture, collaborator creation. |
| Leaf binding | Component-local prepare/clear state for one authored node. |
| `*Animator` | Timeline/tween composition from explicit refs and named motion values. |
| `*Motion` / `*Config` | Named timing, alpha, scale, offset, and fallback values. |
| Collaborators | Buffers, diffing, rendering loops, scroll/layout math, landing target math, and other focused behavior. |

Aggregate `*Bindings` components are injectable because they live under the same view scope. They normally do not need lifecycle attributes unless they subscribe to events or own a lifecycle.

Correct pattern:

```csharp
public sealed class RewardPanelBindings : MonoBehaviour
{
    [SerializeField] private Button _primaryButton;

    public void Validate() { /* fail loud */ }
    internal RewardPanelRefs CreateRefs() { /* pass explicit refs */ }
}

[ViewBind]
public sealed class RewardPanelView : MonoBehaviour
{
    [ViewInject] private GameEventBus _eventBus;
    [ViewInject] private RewardPanelBindings _bindings;
}
```

---

## 6. Baked Component Settings

Scene and prefab authorship owns stable UI configuration:

- Image `type`, `preserveAspect`, `maskable`, raycast flags, materials, and static chrome sprites.
- TMP wrapping, overflow, font style, maskability, and base color.
- ScrollRect direction/movement settings, viewport/content assignment.
- RectTransform anchors, pivots, base size, shadow/glow/shine geometry.

Runtime may update:

- Snapshot-driven content: text, sprite, amount, colors selected by snapshot, enabled/active state.
- Transient presentation: alpha, scale, rotation, anchored position used by animation/layout.
- Dynamic sizes that are genuinely data-dependent.

If a baked setting is wrong, fix the scene, prefab, builder, or editor setup. Do not patch it every render call.

---

## 7. Child Pools

Use editor-populated serialized arrays for authored child pools.

```csharp
[SerializeField] private Transform _poolRoot;

[CollectChildren(nameof(_poolRoot))]
[SerializeField] private ItemView[] _items = Array.Empty<ItemView>();
```

Rules:

- Index equals hierarchy sibling order under the pool root.
- GameObject names are not parsed for matching.
- Each child must have the expected component.
- Collectors mark the host and scene dirty after updating arrays.
- Runtime only reads the serialized array.

Avoid adding extra `*Collection` MonoBehaviours just to fill arrays.

---

## 8. Events and Snapshots

Use an explicit event bus or equivalent message surface:

| Direction | Examples |
| --- | --- |
| UI to gameplay | `StartRequested`, `ConfirmRequested`, `RetryRequested` |
| Gameplay to UI | `StateChanged`, `OutcomeResolved`, `HudSnapshotChanged` |

Views subscribe during lifecycle bind and unsubscribe during lifecycle unbind. No orphaned delegates.

Snapshots should be immutable UI-facing read models. They are built from runtime state plus configuration and consumed by views.

Forbidden in views:

- Reading ScriptableObjects directly for text/color policy when a snapshot should provide it.
- Formatting gameplay copy locally.
- Hard-coded theme colors or labels.
- Mixing event orchestration, serialized scene refs, animation, state diffing, and layout math in one large `MonoBehaviour`.
- Overwriting baked component settings while rendering.

Only the state publisher or flow controller raises snapshot events. UI never pushes state back through snapshots.

---

## 9. Gameplay and Rules

Gameplay rules live in runtime state, flow controllers, and pure rule helpers.

| Piece | Role |
| --- | --- |
| Session state | Mutable runtime state and allowed actions. |
| Flow controller | Handles bus requests, completion callbacks, and publish order. |
| Animation component | Animation only; receives duration/tuning from config. |
| Catalog/config assets | Designer-tuned profiles, copy, motion, themes, resolve tables. |
| `Data/Rules/` helpers | Deterministic layout, outcome, table, and query helpers. |

Gameplay rules stay out of Views and Editor.

---

## 10. Editor vs Runtime

| Editor | Runtime |
| --- | --- |
| Inspectors, setup menus, validators, scene builders, asset repair tools | Never referenced directly. |
| Preprocess build collectors | Populate serialized state before player build. |
| One-off migrations | Deleted after permanent scene/schema migration. |

Player builds must work from serialized data and runtime config only.

---

## 11. Anti-Patterns

Reject these in review:

- Public fields on definitions or ScriptableObjects for convenience.
- Runtime hierarchy search for UI pools or distant dependencies.
- Extra collection MonoBehaviours only to fill arrays.
- String-based child matching in collectors.
- Sorting collected pools by GameObject name at runtime.
- Cross-canvas or cross-root serialized references.
- Views reading raw config for presentation policy.
- God views that own bindings, snapshots, layout, animation, particles, and state.
- Runtime style repair of baked UI settings.
- Lifecycle attributes on pure aggregate binding holders just to make them injectable.
- Editor scripts calling runtime flow as gameplay.
- Duplicate event systems for the same intent.
- Mega inspector lists on the runtime root.
- Silent nulls for required UI wiring.
- Bool-to-index dispatch tables. Prefer explicit `if` or `switch`.
- Secrets or machine-local paths committed in config assets.

---

## 12. Readability

Prefer direct conditionals over clever dispatch:

```csharp
if (!state.CanStart || animator.IsPlaying)
{
    return;
}

StartFlow();
```

Avoid:

```csharp
Action[] actions = { _ => { }, _ => StartFlow() };
actions[Convert.ToInt32(state.CanStart && !animator.IsPlaying)](this);
```

Pure rule helpers must stay deterministic. No hidden `Random`, locator reads, or scene access inside rule tables.

---

## 13. Adding a New UI Pool

1. Create hierarchy under the correct root. Sibling order is index order.
2. Add view with serialized pool root and editor-collected array.
3. Add lifecycle attributes only to lifecycle participants.
4. Extend snapshot factory/catalog if new text, colors, or policy are needed.
5. Subscribe during lifecycle bind; apply only snapshot fields in handlers.
6. Wire editor collection and mark the scene dirty.
7. If serialized references grow past a small handful, split into `*Bindings`, `*View`, `*Animator`, `*Motion`, and focused collaborators before it becomes a god script.

---

## 14. Adding a New Config Field

1. Add `[SerializeField] private` backing field plus get-only property.
2. When renaming a serialized field, migrate existing asset YAML in the same change.
3. Expose through snapshots if UI needs it.
4. Seed defaults through `Create` or `Default`.
5. Document non-obvious designer-facing meaning.

---

## Glossary

| Term | Meaning |
| --- | --- |
| Snapshot | Immutable UI-facing read model for one state update. |
| View scope | UI-root binder that builds same-scope dependency registry and invokes lifecycle callbacks. |
| Definition | Serializable DTO nested in a ScriptableObject or scene config. |
| Catalog | Shared ScriptableObject listing copy, motion, themes, profiles, or resolve data. |
| Pool root | Transform whose ordered children become array indices. |
| Bootstrap | Editor/scene-builder path that creates or resets defaults. |
| Composition root | Runtime owner that starts/stops the session and wires core services. |
