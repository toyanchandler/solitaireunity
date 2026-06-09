# ADR 001 - Default Architecture For Spyke Task

## Decision

Use the simple Handler hypercasual template as the default onsite architecture.

Do not make the task depend on Saneject, Zenject, VContainer, Reflex, or a custom baked binding system.

## Why

Spyke will likely provide a small mechanical Unity task. The priority is to deliver playable behavior quickly while showing production discipline.

The default architecture gives strong senior signals without framework overhead:

- Saveable ScriptableObject data
- Runtime ScriptableObject contexts
- LevelReferenceHolder contracts
- manager-level orchestration
- analytics service boundary
- internal static rules/mappers/appliers
- explicit serialized references

## Consequences

Positive:

- fast task execution
- easy for reviewers to understand
- minimal runtime magic
- clear extension points

Negative:

- does not show editor-time DI tooling by default
- requires discipline to keep managers small

## Follow-up

Mention baked wiring as a future production tooling direction if the conversation goes there.
