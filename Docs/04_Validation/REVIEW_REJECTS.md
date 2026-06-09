# Review Rejects

Reject a change if it includes any of these without a documented exception.

## Runtime dependency shortcuts

- `FindObjectOfType` in gameplay/UI production path
- `GameObject.Find` in gameplay/UI production path
- `Transform.Find` for required scene references
- name-based pool discovery
- cross-root serialized references used to avoid a proper boundary

## Save and analytics violations

- direct analytics SDK calls from gameplay or view scripts
- direct ES3 calls from random gameplay or view scripts
- save/load timing decided by Saveable SO instead of manager/service
- `_constantSaveables[0]` or similar array index semantic logic

## ScriptableObject violations

- Runtime SO unlocks gameplay feature by itself
- Config SO mutated by view at runtime
- SO searches scene
- SO calls analytics or save SDK
- SO becomes a mini GameManager

## Static helper violations

- internal static helper stores scene refs
- internal static helper subscribes to events
- internal static helper reads service locator directly
- static mutable runtime state hidden from reset lifecycle

## Unity hierarchy violations

- required references can be missing silently
- level required spawn point falls back to zero silently
- manager owns far-away unrelated scene references
- UI root references another canvas directly without approved boundary

## LLM and junior risk violations

- no clear extension point for a new save variable
- analytics added in multiple random places
- new feature introduces a new architectural pattern without documentation
- feature can only be understood by reading scene search code
