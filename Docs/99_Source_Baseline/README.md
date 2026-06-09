# Unity Rules

Project-independent Unity architecture rules for scene-authored, data-driven Unity projects.

Use this folder as a portable baseline for new Unity projects. Copy it into a project's `Assets/Scripts/` or `Docs/` folder, then replace only the project-specific names in examples if needed. The rules themselves are intentionally generic.

## Documents

- `SoftwareArchitectureRules.md` - main constitution for code, data, UI, runtime, and editor boundaries.
- `HierarchyWiringRules.md` - scene hierarchy, serialized references, scopes, and child collection rules.
- `AdvancedSoftwareRules.md` - binding lifecycle, aggregate bindings, marker components, presenters, static helpers, and editor scripts.
- `ValidationChecklist.md` - practical done checklist before shipping a feature.

## Core Position

Unity scenes are authored intentionally. Runtime code may animate, show/hide, and render data, but it must not repair scene structure, discover missing dependencies by name, or override baked UI settings as a normal path.

Runtime should read serialized references and immutable snapshots. Editor tooling should collect, validate, migrate, and rebuild scene data before play mode or build time.
