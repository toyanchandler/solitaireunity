# Summary For Spyke Discussion

Use this as a talking track.

## What this architecture is

This is a reusable hypercasual production template. It is not overengineering for one small mechanic. It standardizes the repeated infrastructure decisions that every small Unity game needs:

- save/load
- analytics
- level loading
- character spawn
- camera switching
- runtime state
- UI rendering
- feature extension points

## Why it helps

A junior developer or LLM can add a new feature without rediscovering where things belong.

Example:

- new persistent value goes into Saveable SO
- SaveManager already knows how to save registered providers
- analytics stays in AnalyticsManager
- level references live in LevelReferenceHolder
- runtime current state lives in Runtime SO or runtime state
- tiny logic splits go into internal static rules/appliers instead of new components

## Why not full DI during task

For a 3-hour task, heavy DI or baked binding can waste time. The default approach is explicit and fast. I can discuss editor-time baked validation as a production improvement, but I do not depend on it to finish the task.

## Senior signal

The goal is to deliver a playable mechanic quickly while preserving extension points. I avoid god MonoBehaviours, runtime scene search, scattered save calls, scattered analytics calls, and hidden manager dependencies.

## One sentence

I use a lightweight hypercasual production base where ScriptableObjects provide config, saveable data, and resettable runtime context, managers orchestrate cross-cutting systems, and internal static helpers keep logic split without adding unnecessary Unity components.
