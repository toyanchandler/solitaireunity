# How To Add Runtime State

Use Runtime ScriptableObject state when a prefab or system needs current data and may spawn after old events already happened.

## Use Runtime SO when

- late subscribers need current value
- prefab should not ask for GameManager
- channel identity should be selected in Inspector
- multiple channels of same type may exist
- state must reset between play sessions

## Do not use Runtime SO when

- data is purely local to one instance
- data must be per-enemy and you only have one shared asset
- it is a one-time event with no current state
- logic belongs in a system/rules class

## Template

```csharp
[CreateAssetMenu(menuName = "Runtime State/Game Flow Runtime State")]
public sealed class GameFlowRuntimeState : ResettableRuntimeObject
{
    [SerializeField] private GameState _initialState = GameState.None;

    private GameState _currentState;

    public GameState CurrentState => _currentState;
    public event Action<GameState> Changed;

    public override void ResetRuntimeState()
    {
        _currentState = _initialState;
        Changed = null;
    }

    public void SetState(GameState state)
    {
        if (_currentState == state)
        {
            return;
        }

        _currentState = state;
        Changed?.Invoke(_currentState);
    }
}
```

## Late subscriber pattern

```csharp
private void OnEnable()
{
    _state.Changed += Render;
    Render(_state.CurrentState);
}

private void OnDisable()
{
    _state.Changed -= Render;
}
```

This solves the problem where a newly spawned prefab missed older events.

## Validation

- State resets at play/session start.
- Late subscriber immediately renders current state.
- No gameplay decision lives inside the SO.
- No SDK call lives inside the SO.
