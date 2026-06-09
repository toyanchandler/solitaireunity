using System;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.RunTime
{
    [CreateAssetMenu(fileName = "GameFlowRuntimeState", menuName = "ThisGame/Runtime/GameFlowRuntimeState", order = 0)]
    public sealed class GameFlowRuntimeState : ResettableRuntimeObject
    {
        [SerializeField] private GameState _initialState = GameState.LevelLoaded;

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
