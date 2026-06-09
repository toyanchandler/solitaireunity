using System;
using UnityEngine;

namespace _Game.Scripts.RuntimeState
{
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
}
