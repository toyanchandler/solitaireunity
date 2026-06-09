# ADR 004 - Internal Static Helpers

## Decision

Use internal static helpers for pure logic, mapping, formatting, and stateless appliers.

## Why

Not every separated behavior should be a MonoBehaviour. Too many helper components create hierarchy noise and wiring risk.

Internal static helpers keep logic separate without adding scene components.

## Constraints

Static helpers must:

- be stateless
- receive dependencies as parameters
- not search scene
- not subscribe to events
- not store scene references

## Naming

Prefer:

- `*Rules`
- `*Mapper`
- `*Applier`
- `*Factory`
- `*Formatter`

Avoid `*Controller` for static helpers unless the team accepts the meaning.
