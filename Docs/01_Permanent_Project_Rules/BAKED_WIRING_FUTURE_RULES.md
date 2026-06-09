# Baked Wiring Future Rules

This project does not need baked wiring for the default onsite task flow. However, the architecture can evolve toward Saneject-style editor-time wiring later.

## What baked wiring means

Editor/prebuild resolves dependencies and writes serialized references before runtime.

Runtime then reads already assigned fields.

```text
Editor time
    scan binding metadata
    resolve scene/prefab references
    write serialized fields
    validate graph

Runtime
    use normal serialized references
```

## Why it is useful

- missing dependencies are caught before play/build
- runtime lookup and reflection can be minimized
- Inspector can show references
- Codex and CI can validate graph issues

## Why it is not default for Spyke onsite

- simple mechanical tasks need fast implementation
- setting up tooling may take longer than the task
- reviewers may perceive it as framework work instead of feature work

## Suggested migration path

1. Keep explicit serialized fields and manual validation.
2. Add editor validators for required fields.
3. Add editor collectors for child arrays where needed.
4. Add optional `[BakedFromScope]` for unique semantic dependencies.
5. Add prebuild validation.
6. Only then add automatic bake.

## Hard constraints if implemented

- Generic Button/TMP/Image injection is disallowed unless explicit marker exists.
- Unique semantic components can be baked.
- Collections must be ordered by sibling index, not by GameObject name.
- Missing required references fail validation.
- Optional references must represent real supported variants.
- Runtime service injection remains separate from scene reference baking.
