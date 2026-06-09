using UnityEngine;
using _Game.Scripts.General;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.RunTime;
using Handler.Extensions;

namespace _Game.Scripts.Managers
{
    public sealed class GameStateManager : MonoBehaviour
    {
        [SerializeField] private GameFlowRuntimeState _gameFlowRuntimeState;

        #region Unity Lifecycle Methods

        private void Awake()
        {
            ValidateReferences();
            ChangeGameState(GameState.LevelLoaded);
        }
        
        private void OnEnable() => SubscribeEvents();

        private void OnDisable() => UnsubscribeEvents();

        #endregion

        #region Event Subscription Methods

        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelLoaded += OnLevelLoaded;
            EventManager.InGameEvents.LevelStart += OnLevelStart;
            EventManager.InGameEvents.LevelSuccess += OnLevelSuccess;
            EventManager.InGameEvents.LevelFail += OnLevelFail;
            EventManager.InGameEvents.EndMetaStart += OnEndMetaStart;
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelLoaded -= OnLevelLoaded;
            EventManager.InGameEvents.LevelStart -= OnLevelStart;
            EventManager.InGameEvents.LevelSuccess -= OnLevelSuccess;
            EventManager.InGameEvents.LevelFail -= OnLevelFail;
            EventManager.InGameEvents.EndMetaStart -= OnEndMetaStart;
        }

        #endregion

        private void ValidateReferences()
        {
            if (_gameFlowRuntimeState == null)
            {
                TDebug.LogWarning($"{nameof(GameStateManager)} requires {nameof(_gameFlowRuntimeState)} for late-subscriber game state.");
            }
        }

        #region Event Handlers

        // Changes the game state when the level is loaded
        private void OnLevelLoaded(GameObject go)
        {
            ChangeGameState(GameState.LevelLoaded);
        }

        // Changes the game state when the level starts
        private void OnLevelStart()
        {
            ChangeGameState(GameState.LevelStart);
        }

        // Changes the game state when the level ends in success
        private void OnLevelSuccess()
        {
            ChangeGameState(GameState.Success);
        }

        // Changes the game state when the level ends in failure
        private void OnLevelFail()
        {
            ChangeGameState(GameState.Fail);
        }

        // Changes the game state when the meta end starts
        private void OnEndMetaStart()
        {
            ChangeGameState(GameState.EndMetaStart);
        }

        private void ChangeGameState(GameState state)
        {
            _gameFlowRuntimeState?.SetState(state);
        }

        #endregion
    }
}
