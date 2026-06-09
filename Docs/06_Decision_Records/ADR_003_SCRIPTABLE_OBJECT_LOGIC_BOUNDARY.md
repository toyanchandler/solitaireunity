# ADR 003 - ScriptableObject Logic Boundary

## Decision

ScriptableObjects may contain minimal data safety logic, but not gameplay orchestration.

## Allowed

- current value storage
- initial value storage
- simple clamp
- simple guard
- reset
- capture/apply save data
- Changed event
- snapshot getter

## Forbidden

- deciding feature unlocks
- calling analytics
- calling save SDK from random SOs
- loading scenes
- finding objects
- orchestrating game flow
- acting as GameManager

## Reason

SO assets are excellent for data, context, channels, and persistent state. If too much action logic enters SOs, they become hidden managers and are hard for LLMs/juniors to reason about.
