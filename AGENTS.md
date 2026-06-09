You are working inside an existing Unity template project.

This project is not a blank Unity project.
Do not treat it as a greenfield implementation.

The architecture is LEGO-based, component-driven, inspector-driven, and event-based.




The goal is to create gameplay by composing existing prefabs, existing MonoBehaviours, existing provider scripts, existing ScriptableObject configs, and existing scene objects.

You must prefer scene composition over new code.

---
Do not write new managers. (If not necessary)
Write reaction components (action/response scripts).
Plug into existing interfaces.
Attach to existing prefabs.
Wire references via the Inspector.
----
Core architecture principles:

1. Existing components are the source of truth
- Do not create replacement systems.
- Do not duplicate existing mechanics.
- Do not rewrite managers, providers, event systems, damage systems, interaction systems, upgrade systems, UI systems, or feedback systems.
- Always inspect what already exists first.

2. Scene-first workflow
- This project expects mechanics to be built by adding components to GameObjects.
- Prefer using existing prefabs and wiring references in the inspector.
- Prefer arranging existing objects in the scene.
- Prefer enabling, disabling, duplicating, or configuring existing prefab instances.
- Only write code when scene composition cannot solve the task.

3. LEGO component model
Existing gameplay objects are built from small capability components.

Examples:
- DamageableObject handles damage lifecycle.
- IDamageableAction components react to damage, health changes, and death.
- ClickableObject handles pointer/click lifecycle.
- IClickableAction components react to click down, hold, and release.
- InteractableObject handles interaction lifecycle.
- IInteractableAction components react to interaction.
- Provider classes handle reusable effects such as particles, punch scale, object spawn, sound, visual feedback, and similar feeling systems.

Do not bypass these components.
Attach and configure them.

4. Event-based architecture
The project uses centralized or feature-level events.
When existing event patterns exist, use them.
Do not create direct hard references between unrelated systems unless the existing pattern already does that.
Prefer raising/listening to existing events over adding tightly coupled calls.

5. ScriptableObject/data-driven workflow
If the project already has ScriptableObject configs or data containers, use them.
Do not hardcode balance values, level values, upgrade values, reward amounts, or UI text if an existing config/data path exists.

6. Minimal-code rule
Before writing any new C# file, you must answer:

- Which existing scripts already cover part of this behavior?
- Which existing prefabs can be reused?
- Which scene objects can be duplicated or configured?
- Which inspector references must be assigned?
- What can be achieved with zero new code?
- What is the smallest missing adapter, if any?

New scripts are allowed only as small adapters or missing action components.
They must not introduce a new architecture.

7. FormerlySerializedAs is forbidden
- Do not use FormerlySerializedAs.
- If you need to rename, replace, or remove a serialized field, first inspect the current value in the active scene, relevant prefab instances, prefab assets, and project assets.
- Record the current serialized value somewhere before making the change.
- After the change, verify that the exact value is restored in the scene or asset where it existed.
- Do not rely on Unity serialization migration attributes as a shortcut.

8. Do not create parallel systems
Forbidden unless explicitly requested:
- New GameManager
- New CombatManager
- New UIManager
- New InputManager
- New EventManager
- New Damage system
- New Interaction system
- New Upgrade system
- New Currency system
- New pooling system
- New scene bootstrap system

9. Preserve existing scene structure
The current scene is already populated.
Do not delete or rebuild the scene from scratch.
Do not replace existing root objects unless required.
Work by modifying, duplicating, wiring, and extending the existing scene.

10. Implementation order
For every task, follow this order:

Step 1: Inspect existing scripts, prefabs, and scene hierarchy.
Step 2: Identify relevant existing architecture.
Step 3: Propose a zero-new-code scene composition plan.
Step 4: If impossible, propose the smallest adapter component.
Step 5: Implement only the approved minimal changes.
Step 6: List exact inspector assignments and prefab changes.
Step 7: If serialized fields changed, verify the previous inspector values were restored without FormerlySerializedAs.
Step 8: Verify no duplicate systems were created.

11. Output format
Before changing code, output this:

Existing assets/scripts found:
- ...

Reusable components:
- ...

Scene composition plan:
- ...

Zero-code solution:
- Possible / Not possible
- Explanation

Minimal code required:
- None / Adapter only
- File name
- Reason

Forbidden changes avoided:
- No new manager
- No duplicate damage system
- No duplicate interaction system
- No duplicate event bus
- No scene rebuild


====


Important project rule:

This is a LEGO-based Unity template, not a blank project.

Do not solve this task by writing a new system from scratch.

First inspect the existing scripts, prefabs, ScriptableObjects, and scene hierarchy.

Your priority order is:
1. Use existing prefab/component composition.
2. Configure existing inspector references.
3. Duplicate and adapt existing scene objects.
4. Use existing event/provider/action systems.
5. Write only the smallest adapter script if absolutely necessary.

You must not create new managers or replacement architecture.

Before coding, explain how the mechanic can be built using existing components.


===


## Required Runtime Validation After Scene/Prefab Changes

After modifying prefabs or scene objects, verify that all required runtime dependencies exist.

Check:
- Required manager objects exist in the active scene.
- Required tags/layers are assigned when an existing component depends on them.
- Inspector enum selections match actual scene setup.
- Referenced prefabs are assigned.
- Required action components are registered in the correct action/component arrays.
- The selected Clicked Object / target mode can actually resolve at runtime.
- No missing references are left in newly added components.
- The feature can be triggered through the existing lifecycle without adding duplicate systems.

If a required dependency is missing, report it clearly instead of silently assuming it exists.

Treat this task as scene assembly and component composition, not feature programming.



====

If you create a new system (current is not enough) be sure you read all docs document
