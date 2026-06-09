using System.Collections.Generic;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.UI.Screens
{
    public class UIViewManager : SerializedMonoBehaviour
    {
        #region Variables

        [SerializeField] private Dictionary<GameState, GameObject> _gameStateDictionary;

        private static readonly GameState[] RequiredPanelStates =
        {
            GameState.LevelLoaded,
            GameState.LevelStart,
            GameState.Fail
        };

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ValidatePanelMappings();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            OpenPanel(GameState.LevelLoaded);
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            EventManager.InGameEvents.GameStarted += HandleOnGameStart;
            EventManager.InGameEvents.LevelSuccess += HandleOnLevelEnd;
            EventManager.InGameEvents.LevelStart += HandleOnLevelStart; 
            EventManager.InGameEvents.LoadLevel += HandleOnGameStart;
            EventManager.InGameEvents.LevelFail += HandleOnLevelFail;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.InGameEvents.GameStarted -= HandleOnGameStart;
            EventManager.InGameEvents.LevelSuccess -= HandleOnLevelEnd;
            EventManager.InGameEvents.LevelStart -= HandleOnLevelStart;
            EventManager.InGameEvents.LoadLevel -= HandleOnGameStart;
            EventManager.InGameEvents.LevelFail -= HandleOnLevelFail;
        }

        #endregion

        #region Event Handlers

        private void HandleOnGameStart()
        {
            OpenPanel(GameState.LevelLoaded);
        }

        private void HandleOnLevelStart()
        {
            OpenPanel(GameState.LevelStart);
        }
        
        private void HandleOnLevelEnd()
        {
            OpenPanel(GameState.Success, GameState.LevelEnd);
        }
        
        private void HandleOnLevelFail()
        {
            OpenPanel(GameState.Fail);
        }

        #endregion

        #region Panel Management

        private void OpenPanel(GameState state)
        {
            OpenPanel(state, state);
        }

        private void OpenPanel(GameState state, GameState fallbackState)
        {
            CloseAllPanels();

            if (TryGetPanel(state, out GameObject panel) ||
                (fallbackState != state && TryGetPanel(fallbackState, out panel)))
            {
                panel.SetActive(true);
                return;
            }

            TDebug.LogWarning($"{nameof(UIViewManager)} has no panel mapping for state '{state}'.");
        }

        private void CloseAllPanels()
        {
            if (_gameStateDictionary == null)
            {
                return;
            }

            foreach (var panel in _gameStateDictionary.Values)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
        }

        private bool TryGetPanel(GameState state, out GameObject panel)
        {
            panel = null;

            if (_gameStateDictionary == null)
            {
                TDebug.LogWarning($"{nameof(UIViewManager)} requires panel mappings.");
                return false;
            }

            if (!_gameStateDictionary.TryGetValue(state, out panel) || panel == null)
            {
                return false;
            }

            return true;
        }

        private void ValidatePanelMappings()
        {
            if (_gameStateDictionary == null || _gameStateDictionary.Count == 0)
            {
                TDebug.LogWarning($"{nameof(UIViewManager)} requires panel mappings.");
                return;
            }

            foreach (GameState state in RequiredPanelStates)
            {
                if (!_gameStateDictionary.TryGetValue(state, out GameObject panel) || panel == null)
                {
                    TDebug.LogWarning($"{nameof(UIViewManager)} is missing a panel mapping for state '{state}'.");
                }
            }

            bool hasSuccessPanel =
                (_gameStateDictionary.TryGetValue(GameState.Success, out GameObject successPanel) && successPanel != null) ||
                (_gameStateDictionary.TryGetValue(GameState.LevelEnd, out GameObject levelEndPanel) && levelEndPanel != null);

            if (!hasSuccessPanel)
            {
                TDebug.LogWarning($"{nameof(UIViewManager)} is missing a success/endgame panel mapping.");
            }

            foreach (KeyValuePair<GameState, GameObject> panelEntry in _gameStateDictionary)
            {
                if (panelEntry.Value == null)
                {
                    TDebug.LogWarning($"{nameof(UIViewManager)} has a null panel reference for state '{panelEntry.Key}'.");
                }
            }
        }

        #endregion
    }
}
