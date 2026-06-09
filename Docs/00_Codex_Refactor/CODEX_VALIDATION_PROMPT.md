# Codex Validation Prompt

Use this prompt after migration or after each large refactor.

```text
Validate the Unity project against the Handler Hypercasual Architecture rules.

Produce a report with these sections:

1. Compile and import safety
- Are there obvious missing namespaces or broken serialized renames?
- Did any file introduce editor-only API into runtime assemblies?

2. Runtime lookup violations
Search for:
- FindObjectOfType
- GameObject.Find
- Transform.Find
- GetComponentsInChildren in gameplay/UI production paths
- name-based child lookup
- string-based pool discovery

Explain each occurrence and whether it is allowed. Controlled editor validators are allowed. Runtime production logic is not.

3. Save pipeline
- List all ISaveableProvider implementations.
- Confirm SaveManager has explicit named saveables or clear groups.
- Flag array index semantic access like _constantSaveables[0].
- Confirm Easy Save calls are isolated to save backend/provider layer.

4. Analytics pipeline
- Confirm analytics SDK calls are isolated to IAnalyticsService implementations.
- Confirm AnalyticsManager listens to gameplay events and does not mutate gameplay state.
- Flag analytics calls inside gameplay/view classes.

5. ScriptableObject boundaries
- List Runtime SO classes.
- Confirm each has reset behavior.
- Flag SOs with gameplay orchestration, SDK calls, scene lookup, or large action logic.
- Allow simple guards, clamp, snapshot, Changed event, Capture/Apply data.

6. Level contract
- Confirm each loaded level root expects LevelReferenceHolder.
- Confirm CharacterManager uses LevelReferenceHolder and does not deep-search.
- Flag silent Vector3.zero fallback for required spawn point.

7. Static helper rules
- List internal static classes.
- Confirm they are stateless and parameter-driven.
- Flag static runtime fields, scene lookup, event subscriptions, or service locator reads.

8. UI and view rules
- Views should not save, analytics-log, or compute gameplay decisions.
- Views render current state/snapshots and own local serialized refs.

9. Review rejects
List any blocker that should reject the change.

10. Final status
Return one of:
- PASS
- PASS_WITH_WARNINGS
- FAIL

For FAIL, list the minimum changes required to pass.
```
