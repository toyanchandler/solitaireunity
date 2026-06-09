# ADR 006 - Save and Analytics Pipeline

## Decision

Save and analytics remain cross-cutting manager/service responsibilities.

## Save

Persistent state lives in Saveable SOs or save data classes. SaveManager decides when to save and load.

## Analytics

Gameplay publishes events. AnalyticsManager listens and logs through IAnalyticsService.

## Why

This lets juniors and LLMs add new data or analytics without spreading infrastructure calls across feature scripts.

## Consequences

Positive:

- adding GameAnalytics or another SDK is localized
- save timing is predictable
- new persistent variables have a known path

Negative:

- managers can grow if not reviewed
- event surface must stay clean
