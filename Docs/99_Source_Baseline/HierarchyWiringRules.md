# Hierarchy Wiring Rules

These rules define how Unity scene references are serialized and how systems communicate without turning the hierarchy into a hidden dependency graph.

## SerializeField

- A component may reference components on the same GameObject or descendants in its own subtree.
- Shared ScriptableObjects under `Assets/Config/` are allowed.
- The runtime root may reference same-root runtime components and one explicit config source.
- Never drag references across canvas roots, runtime roots, scene branches, or unrelated feature subtrees.

## Cross-System Communication

Use one of these paths:

1. **Composition root / runtime locator** for session-owned services.
2. **Event bus** for UI intents and gameplay-to-UI snapshots.
3. **Same-scope injection** for components under the same UI root.
4. **Shared ScriptableObjects** for designer-authored data.

Do not serialize distant scene branches just to avoid a proper boundary.

## View Scopes

Each UI root or canvas has exactly one view scope.

The scope:

- Discovers components only under its own subtree.
- Registers same-scope components by type.
- Injects lifecycle participants.
- Calls bind/unbind callbacks.
- Subscribes to runtime-ready/runtime-stopped signals if the project has a runtime locator.

Views that need lifecycle use a lifecycle marker/attribute. Pure aggregate `*Bindings` components stay injectable-only unless they subscribe to events or own real lifecycle behavior.

Avoid legacy `*UiHost` scripts that hold serialized references to unrelated canvases.

## Child Pools

Use editor-collected arrays, not runtime hierarchy search.

```csharp
[SerializeField] private Transform _itemPoolRoot;

[CollectChildren(nameof(_itemPoolRoot))]
[SerializeField] private ItemView[] _items;
```

Contract:

- Ordering equals hierarchy sibling index.
- GameObject names are not parsed.
- Collection happens in editor tooling or build preprocess.
- Runtime only reads the serialized array.
- Collectors mark the component and scene dirty.

Do not create permanent runtime `*Collection` components just to populate arrays.

## Required References

Default to required wiring.

Use optional wiring only when:

- The same script intentionally supports multiple scene variants.
- The feature can degrade validly without the object.
- The missing object is expected in a supported build.

Once a migration is complete, remove optional paths that only masked incomplete scene work.

## Runtime

Runtime code may:

- Read serialized references.
- Subscribe/unsubscribe to explicit events.
- Render snapshot data.
- Animate transient values.

Runtime code must not:

- Call `Transform.Find`, `GameObject.Find`, or deep child search for production UI.
- Parse child names to build pools.
- Sort serialized pools by name.
- Repair scene wiring on launch.
- Reach into another canvas/root by serialized field.

## Validation

Before play mode or build:

1. Run view-specific collectors.
2. Confirm every UI root has exactly one view scope.
3. Confirm child arrays are populated in expected sibling order.
4. Confirm no required serialized reference is null.
5. Confirm no active hierarchy uses deprecated host or migration scripts.
