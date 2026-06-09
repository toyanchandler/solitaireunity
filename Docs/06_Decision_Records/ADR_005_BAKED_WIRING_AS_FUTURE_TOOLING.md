# ADR 005 - Baked Wiring As Future Tooling

## Decision

Baked wiring is a future enhancement, not the default task path.

## What it would solve

- editor/prebuild dependency validation
- missing references before runtime
- less runtime reflection/search
- Inspector-visible dependency graph

## Why not default now

- setup time may exceed onsite task needs
- simple explicit wiring is faster
- task evaluation rewards feature delivery first

## Future version

A future `BakedViewWiring` tool may support:

- `[BakedFromScope]` unique semantic component fields
- editor-collected child arrays
- prebuild validation
- context-rich dependency errors
- no generic Button/TMP/Image injection unless marked explicitly

## Relationship to DI

This is not a full runtime DI container. It is editor-time scene reference baking plus validation. Runtime services remain explicit through managers, composition root, or event bus.
