# Codex Execution Prompt

Copy this prompt into Codex when starting the migration.

```text
You are refactoring this Unity hypercasual project toward the architecture described in Docs/Architecture.

Do not introduce external DI frameworks.
Do not rewrite gameplay from scratch.
Do not change behavior unless the current behavior is clearly a bug and you document it.

Refactor in the following order:
1. Inventory current managers, saveables, runtime SOs, EventManager events, LevelReferenceHolder usage.
2. Encapsulate public Inspector fields as private serialized fields where safe.
3. Make SaveManager safer by removing array index semantics like _constantSaveables[0].
4. Keep AnalyticsManager as the single analytics listener and use IAnalyticsService boundary.
5. Standardize Runtime ScriptableObject reset behavior.
6. Standardize LevelReferenceHolder and CharacterManager contract.
7. Clean CameraManager state mapping.
8. Extract pure logic into internal static Rules, Mappers, Appliers where helpful.
9. Run validation checklist and produce a validation report.

Hard constraints:
- No FindObjectOfType, GameObject.Find, Transform.Find, or deep hierarchy search in production runtime path.
- No save SDK calls from random gameplay classes.
- No analytics SDK calls from random gameplay classes.
- No ScriptableObject gameplay orchestration.
- No silent nulls for required references.
- No public mutable fields just for Inspector exposure.
- No static helper with hidden runtime state.

After each phase, report:
- files changed
- behavior preserved
- validation issues found
- unresolved risks
```
