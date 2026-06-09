# Advanced Software Rules

This document expands the base architecture rules for binding lifecycle, marker components, aggregate bindings, static helpers, presenters, and editor scripts.

---

## 1. Runtime Binding Lifecycle

The view graph is bound by view scopes, not by a central UI host.

Runtime flow:

1. The view scope builds a same-scope component registry from every `MonoBehaviour` under that UI root.
2. Bind discovery selects only classes marked for lifecycle.
3. When runtime is ready, bindable components receive injected fields.
4. After all fields are assigned, after-bind callbacks run.
5. On disable, runtime stop, or teardown, before-unbind callbacks run.

Important consequences:

- A component can be injectable without being a lifecycle participant.
- The lifecycle marker means "call my lifecycle", not "make me discoverable by type".
- Aggregate binding components should usually be injectable only.
- If injection fails, first check scene placement under the same view scope. Do not add lifecycle markers as a workaround.

Correct aggregate pattern:

```csharp
public sealed class OutcomePopupBindings : MonoBehaviour
{
    [SerializeField] private OutcomePopupRootBinding _root;
    [SerializeField] private OutcomePopupIconBinding _icon;

    public void Validate() { /* fail loud */ }
    internal OutcomePopupRefs CreateRefs() { /* create typed refs */ }
}

[ViewBind]
public sealed class OutcomePopupView : MonoBehaviour
{
    [ViewInject] private GameEventBus _eventBus;
    [ViewInject] private OutcomePopupBindings _bindings;

    [ViewAfterInject]
    private void Connect()
    {
        _bindings.Validate();
    }
}
```

Wrong aggregate pattern:

```csharp
[ViewBind]
public sealed class OutcomePopupBindings : MonoBehaviour
{
    // This has no event lifecycle and should not be invoked by the scope.
}
```

---

## 2. Marker Components

Marker components are allowed when the hierarchy has semantic nodes that runtime must address directly.

Good marker:

- Names one authored scene node.
- Wraps required sibling components.
- Owns tiny scene-state helpers like `CaptureHome`, `SetAlpha`, or `RestoreHome`.
- Throws clear errors when a required sibling component is missing.

Bad marker:

- Parses names.
- Searches outside its GameObject.
- Knows gameplay rules.
- Becomes a broad utility without a clear scene contract.

Prefer panel-local marker bases first. Promote to shared only after multiple panels prove the same contract.

---

## 3. Aggregate Bindings

Use aggregate bindings when a controller view would otherwise need many serialized fields or many injected fields.

Good candidates:

- Popup controllers.
- Overlays.
- Multi-part panels with authored child nodes.
- Views that create typed refs/presenters.

Bad candidates:

- Leaf card views.
- Simple buttons.
- Small text views.
- Components with one or two local references.

Aggregate responsibilities:

- Hold serialized scene references under the same subtree.
- Validate required references.
- Capture authored home state.
- Build typed ref objects or focused collaborators.
- Keep optional references rare and explicit.

Aggregate must not:

- Subscribe to events.
- Hold gameplay state.
- Call runtime locator directly.
- Search the hierarchy in play mode.
- Be marked for lifecycle unless it owns real lifecycle behavior.

Rule of thumb:

If a class has more than 7-8 serialized scene references and also has event or presenter logic, split it into:

- `*View` for lifecycle, events, and orchestration.
- `*Bindings` for authored scene references.
- `*Refs` or focused collaborators for presenter input.
- `*Animator` for timeline composition only.
- `*Motion` / `*Config` for named timings, offsets, alpha values, and fallback values.

The split must preserve baked scene ownership. Bindings may prepare transient alpha/scale/active state, but they must not repair authored UI settings such as maskability, raycast flags, image type, TMP wrapping/style, anchors, pivots, or chrome geometry at runtime.

---

## 4. Required vs Optional Bindings

Default to required.

Use required when:

- The object exists in the current hierarchy.
- The feature cannot work correctly without it.
- Missing wiring should fail before shipping.

Use optional only when:

- The same code intentionally supports multiple scene variants.
- The feature is genuinely degraded but valid without that object.
- The missing object is expected in some builds.

Do not leave optional injections or nullable serialized fields just because a migration used to be incomplete.

Validation should fail loud:

```csharp
private void Require(UnityEngine.Object value, string fieldName)
{
    if (value == null)
    {
        throw new InvalidOperationException(name + " requires " + fieldName + ".");
    }
}
```

---

## 5. Attribute Usage

### Lifecycle marker

Use on classes that should receive injection lifecycle callbacks.

Allowed:

- Views that subscribe to event bus snapshots.
- Button actions that raise intents.
- Base classes whose subclasses all need the same lifecycle.

Avoid:

- Pure serialized binding holders.
- Marker components with no event lifecycle.
- Plain collaborator objects.

### Injection marker

Use for same-scope runtime dependencies.

Allowed injected types:

- Event bus.
- Runtime services explicitly exposed by the composition root.
- A unique component type under the same view scope.

Avoid:

- Injecting broad base classes when multiple instances exist.
- Optional injection to hide bad hierarchy wiring.
- Cross-canvas coupling.

### After-bind callback

Use for:

- Validation after all dependencies exist.
- Event subscription.
- Presenter/collaborator creation.
- Initial reset to a known UI state.

Do not use for:

- Hierarchy search.
- Scene rebuild.
- One-time migration.

### Before-unbind callback

Use for:

- Event unsubscribe.
- Tween kill.
- Presenter reset.
- Runtime-only cleanup.

The method must be idempotent. It can run during stop, disable, or editor capture cleanup.

### Child collection marker

Use only for authored child pools that must be serialized in sibling order. It is editor-populated metadata. Runtime must only read the serialized array.

---

## 6. Static Classes

Static classes are allowed for pure policies and stateless operations. They are not a place to hide lifecycle or mutable session state.

Good static classes:

- `*Rules` - deterministic domain decisions.
- `*Palette` - deterministic color selection.
- `*AnimationConfig` - constants and catalog defaults.
- `*Animator` - stateless tween construction from explicit refs.
- `*Applier` - maps a snapshot onto already-bound refs.
- Tiny reusable UI utility classes.

Bad static classes:

- Hold scene references.
- Subscribe to events.
- Read runtime locator directly for presentation.
- Mutate hidden global state.
- Replace a missing presenter or service boundary.

Allowed example:

```csharp
internal static class OutcomePopupContentApplier
{
    public static void Apply(OutcomePopupRefs refs, OutcomeSnapshot snapshot)
    {
        refs.Icon.ApplySprite(snapshot.Icon, snapshot.IconTint, 1f);
        refs.ResultText.Apply(snapshot.ResultText, snapshot.TextColor);
    }
}
```

The static class receives all dependencies as parameters. It does not discover the scene.

---

## 7. Presenters and Refs

Presenter classes own flow decisions for a view, but they should not know Unity hierarchy.

Presenter may:

- Consume snapshots.
- Decide which animation path to run.
- Track small presentation state.
- Call methods on a typed refs object.

Presenter must not:

- Use `GetComponent`, `Find`, `Resources`, or scene names.
- Own serialized fields.
- Mutate gameplay state directly.

Refs objects should be immutable after construction:

```csharp
internal sealed class OutcomePopupRefs
{
    public OutcomePopupRootBinding Root { get; }
    public OutcomePopupIconBinding Icon { get; }

    public OutcomePopupRefs(
        OutcomePopupRootBinding root,
        OutcomePopupIconBinding icon)
    {
        Root = root;
        Icon = icon;
    }
}
```

Do not use public mutable field bags for presenter refs. If a ref can change, there must be a named method that explains why.

---

## 8. Editor Scripts and Migrations

Editor scripts are for repeatable tooling, not permanent cleanup leftovers.

Keep:

- Inspectors.
- Asset pipeline tools.
- Build preprocess collectors.
- Supported validators and project setup checks.
- Scene builders that regenerate supported hierarchy.

Delete after use:

- One-off migrations.
- Temporary rebuilders.
- Screenshot/export scripts that only served a local debug pass.
- Play-mode command helpers that are not part of the supported validation path.

If a hierarchy change is now permanent, encode it in:

- The scene.
- The scene builder, if a builder owns the same objects.
- The relevant validator or setup check.
- The relevant binding aggregate.

Do not keep a migration around to explain history.

---

## 9. Player Build Expectations

Player builds do not run editor collection or migration scripts.

Before building:

- Scene references must already be serialized.
- Child collection arrays must already be populated or collected by preprocess build.
- Optional bindings must represent real supported variants, not missing scene work.
- No runtime script should depend on editor menus, editor-only rebuilders, or hierarchy search.

Safe in player:

- View scope discovery under the active UI root.
- Lifecycle/injection callbacks.
- Serialized aggregate bindings.
- Static helpers that receive explicit refs/snapshots.

Not safe in player:

- `UnityEditor` APIs.
- One-off migration scripts.
- Runtime hierarchy search.
- Assuming editor scripts will repair missing references at launch.

---

## 10. Review Checklist

Before merging a UI architecture change:

1. Does every lifecycle-marked class actually need lifecycle callbacks?
2. Does every required injected dependency exist once in the same scope?
3. Are optional dependencies backed by a real supported scene variant?
4. Are large field lists moved into a local `*Bindings` aggregate?
5. Are marker components named for scene semantics, not generic utility ideas?
6. Are refs immutable after construction?
7. Do static classes receive all dependencies as parameters?
8. Did editor-only migration/debug scripts get deleted after permanent scene/schema change?
9. Does the player path work with serialized data only?
