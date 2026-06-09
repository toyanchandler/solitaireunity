# How To Add A UI View

UI views render state. They do not own save, analytics, or gameplay decisions.

## Recommended hierarchy

```text
MainCanvas
└── ResultPanel
    ├── ResultPanelView
    ├── ResultPanelBindings
    ├── ResultPanelAnimator
    ├── TitleText
    ├── RewardText
    └── ClaimButton
```

## Responsibilities

### View

- subscribe/unsubscribe to runtime state or event bus
- render snapshots/current state
- publish UI intents
- call bindings and animator

### Bindings

- hold serialized references
- validate required refs
- expose typed refs or properties

### Animator/Applier

- animate or apply explicit refs
- no gameplay decisions

## Pattern

```csharp
private void OnEnable()
{
    _state.Changed += Render;
    Render(_state.CurrentSnapshot);
}

private void OnDisable()
{
    _state.Changed -= Render;
}
```

## Do not

- call SaveManager from a view
- call Analytics SDK from a view
- calculate reward rules in a view
- use `GameObject.Find` or child-name parsing
- inject generic Button/TMP by type when bindings can own explicit refs

## Validation

- View updates when state changes.
- Late enable renders current state.
- Button events are unsubscribed.
- Missing binding gives clear error or warning.
