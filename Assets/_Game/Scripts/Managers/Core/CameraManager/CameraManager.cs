using System.Collections.Generic;
using _Game.Scripts.General;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using Handler.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Cinemachine;

namespace _Game.Scripts.Managers
{
    public sealed class CameraManager : SerializedMonoBehaviour
    {
        #region Variables

        [SerializeField] private Dictionary<GameState, CinemachineVirtualCamera> _virtualCameraDictionary =
            new Dictionary<GameState, CinemachineVirtualCamera>();

        private bool _hasCurrentState;
        private GameState _currentState;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        private void ValidateReferences()
        {
            if (_virtualCameraDictionary == null || _virtualCameraDictionary.Count == 0)
            {
                TDebug.LogWarning($"{nameof(CameraManager)} requires at least one game-state camera mapping.");
                return;
            }

            foreach (KeyValuePair<GameState, CinemachineVirtualCamera> cameraEntry in _virtualCameraDictionary)
            {
                if (cameraEntry.Value == null)
                {
                    TDebug.LogWarning($"{nameof(CameraManager)} has a missing virtual camera for state '{cameraEntry.Key}'.");
                }
            }
        }

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            EventManager.InGameEvents.GameStarted += HandleOnGameStart;
            EventManager.InGameEvents.LevelStart += HandleOnLevelStart;
            EventManager.InGameEvents.LevelSuccess += HandleOnLevelEnd;
            EventManager.InGameEvents.LevelFail += HandleOnLevelFail;
            EventManager.InGameEvents.EndMetaStart += HandleOnEndMetaStart;
            EventManager.InGameEvents.LevelLoaded += HandleOnLevelLoaded;
        }
 

        private void UnsubscribeFromEvents()
        {
            EventManager.InGameEvents.GameStarted -= HandleOnGameStart;
            EventManager.InGameEvents.LevelStart -= HandleOnLevelStart;
            EventManager.InGameEvents.LevelSuccess -= HandleOnLevelEnd;
            EventManager.InGameEvents.LevelFail -= HandleOnLevelFail;
            EventManager.InGameEvents.EndMetaStart -= HandleOnEndMetaStart;
            EventManager.InGameEvents.LevelLoaded -= HandleOnLevelLoaded;
        }

        #endregion

        #region Event Handlers

        private void HandleOnGameStart()
        {
            SwitchVirtualCamera(GameState.LevelLoaded);
        }
        
        private void HandleOnLevelStart()
        {
            SwitchVirtualCamera(GameState.LevelStart);
        }
        private void HandleOnLevelLoaded(GameObject arg0)
        {
            HandleOnGameStart();
        }
        
        private void HandleOnLevelEnd()
        {
            SwitchVirtualCamera(GameState.LevelEnd);
        }

        private void HandleOnEndMetaStart()
        {
            SwitchVirtualCamera(GameState.EndMetaStart);
        }
        
        private void HandleOnLevelFail()
        {
            SwitchVirtualCamera(GameState.Fail);
        }

        #endregion

        #region Camera Management

        private void SwitchVirtualCamera(GameState state)
        {
            if (_hasCurrentState && _currentState == state)
            {
                return;
            }

            _hasCurrentState = true;
            _currentState = state;

            DisableAllCameras();

            if (!_virtualCameraDictionary.TryGetValue(state, out CinemachineVirtualCamera virtualCamera) || virtualCamera == null)
            {
                TDebug.LogWarning($"{nameof(CameraManager)} has no virtual camera for state '{state}'.");
                return;
            }

            virtualCamera.gameObject.SetActive(true);
        }

        private void DisableAllCameras()
        {
            foreach (CinemachineVirtualCamera cam in _virtualCameraDictionary.Values)
            {
                if (cam != null)
                {
                    cam.gameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}
