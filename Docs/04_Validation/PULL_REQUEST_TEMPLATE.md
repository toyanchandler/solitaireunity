# Pull Request Checklist

## Behavior

- [ ] Existing game flow still works.
- [ ] Level load works.
- [ ] Level start works.
- [ ] Success path works.
- [ ] Fail path works if supported.
- [ ] Save/load works.
- [ ] Analytics still logs required events.

## Architecture

- [ ] No runtime scene search was added.
- [ ] No direct analytics SDK call was added outside analytics service layer.
- [ ] No direct save SDK call was added outside save layer.
- [ ] ScriptableObjects did not gain orchestration logic.
- [ ] New static helpers are stateless.
- [ ] New MonoBehaviours own lifecycle or scene references for a clear reason.

## Data

- [ ] Persistent data is in Saveable SO or save data class.
- [ ] Runtime state resets correctly.
- [ ] Config data is not mutated by views.

## Level/UI

- [ ] LevelReferenceHolder contract is satisfied.
- [ ] UI views render state/snapshots and do not perform gameplay decisions.
- [ ] Required serialized references are assigned.

## Notes

Migration risks:

Validation performed:

Known warnings:
