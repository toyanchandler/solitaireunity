using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Managers
{
    public sealed class CameraManager : SerializedMonoBehaviour
    {
        [SerializeField]
        private Dictionary<GameState, CinemachineVirtualCamera> _virtualCamerasByState = new();

        private GameState? _currentState;

        private void OnEnable()
        {
            EventManager.InGameEvents.GameStarted += HandleGameStarted;
            EventManager.InGameEvents.LevelStart += HandleLevelStarted;
            EventManager.InGameEvents.LevelSuccess += HandleLevelSucceeded;
            EventManager.InGameEvents.LevelFail += HandleLevelFailed;
            EventManager.InGameEvents.EndMetaStart += HandleEndMetaStarted;
            EventManager.InGameEvents.LevelLoaded += HandleLevelLoaded;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.GameStarted -= HandleGameStarted;
            EventManager.InGameEvents.LevelStart -= HandleLevelStarted;
            EventManager.InGameEvents.LevelSuccess -= HandleLevelSucceeded;
            EventManager.InGameEvents.LevelFail -= HandleLevelFailed;
            EventManager.InGameEvents.EndMetaStart -= HandleEndMetaStarted;
            EventManager.InGameEvents.LevelLoaded -= HandleLevelLoaded;
        }

        private void HandleGameStarted() => SwitchVirtualCamera(GameState.LevelLoaded);
        private void HandleLevelStarted() => SwitchVirtualCamera(GameState.LevelStart);
        private void HandleLevelSucceeded() => SwitchVirtualCamera(GameState.LevelEnd);
        private void HandleLevelFailed() => SwitchVirtualCamera(GameState.Fail);
        private void HandleEndMetaStarted() => SwitchVirtualCamera(GameState.EndMetaStart);
        private void HandleLevelLoaded(GameObject levelRoot) => SwitchVirtualCamera(GameState.LevelLoaded);

        private void SwitchVirtualCamera(GameState state)
        {
            if (_currentState == state)
            {
                return;
            }

            _currentState = state;
            DisableAllCameras();

            if (!_virtualCamerasByState.TryGetValue(state, out CinemachineVirtualCamera camera) || camera == null)
            {
                Debug.LogWarning($"CameraManager has no virtual camera for state '{state}'.", this);
                return;
            }

            camera.gameObject.SetActive(true);
        }

        private void DisableAllCameras()
        {
            foreach (CinemachineVirtualCamera camera in _virtualCamerasByState.Values)
            {
                if (camera != null)
                {
                    camera.gameObject.SetActive(false);
                }
            }
        }
    }
}
