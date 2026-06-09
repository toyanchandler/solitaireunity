using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public class RunnerKeyboardInput : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private bool smoothTransitionMode = true;

        #endregion
        
        #region Private Variables
        
        private CoroutineService _coroutineHandler;

        #region Input Variables

        [SerializeField] private InputDataContainer.TransformSettings _transformSettings;
        [SerializeField] private InputDataContainer.InputSettings _inputSettings;
        [SerializeField] private InputDataContainer.InputConstants _inputConstants;
        private InputDataContainer.InputInternalState _inputInternalState;

        #endregion

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _coroutineHandler = new CoroutineService(this);
            
            GetDefaults();
        }

        private void OnEnable() => SubscribeEvents();

        private void OnDisable() => UnsubscribeEvents();

        #endregion

        #region Subscribe/Unsubscribe Events

        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart += StartInput;
            EventManager.InGameEvents.LevelSuccess += StopInput;
            EventManager.InGameEvents.LevelFail += StopInput;
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart -= StartInput;
            EventManager.InGameEvents.LevelSuccess -= StopInput;
            EventManager.InGameEvents.LevelFail -= StopInput;
        }

        #endregion

        #region Private Methods

        private void GetDefaults()
        {
            if (_transformSettings._targetTransform == null)
                _transformSettings._targetTransform = transform;
        }
        
        private void StartInput()
        {
            _coroutineHandler.StartUpdateRoutine(InputUpdate, () => true);
            _coroutineHandler.StartUpdateRoutine(InertiaUpdate, () => true);
        }

        private void StopInput()
        {
            _coroutineHandler.StopAll();
        }

        private void InputUpdate()
        {
            var horizontalInput = Input.GetAxis("Horizontal") * _inputSettings.BaseSensitivity;
            HandleHorizontalInput(horizontalInput);

            _inputInternalState.InertiaTimer = _inputConstants.InertiaDuration;
        }

        private void InertiaUpdate()
        {
            if (_inputInternalState.InertiaTimer > 0)
            {
                var _inertiaMultiplier = _inputInternalState.InertiaTimer / _inputConstants.InertiaDuration;
                HandleHorizontalInputWithDamping(_inputInternalState.LastHorizontalInput * _inertiaMultiplier);
                _inputInternalState.InertiaTimer -= Time.deltaTime;
            }
            else
            {
                _inputInternalState.InertiaTimer = 0;
                _inputInternalState.LastHorizontalInput = 0;
                _inputInternalState.CurrentHorizontalInput = 0;
            }
        }

        private void HandleHorizontalInput(float horizontalInput)
        {
            _inputInternalState.LastHorizontalInput = horizontalInput;
            HandleHorizontalInputWithDamping(_inputInternalState.LastHorizontalInput);
        }

        private void HandleHorizontalInputWithDamping(float newHorizontalInput)
        {
            _inputInternalState.CurrentHorizontalInput = smoothTransitionMode ? Mathf.Lerp(_inputInternalState.CurrentHorizontalInput, newHorizontalInput, _inputConstants.InputLag) : newHorizontalInput;

            HandleSplineModeInactive();
        }

        private void HandleSplineModeInactive()
        {
            var position = _transformSettings._targetTransform.position;
            var clampedX = Mathf.Clamp(position.x + _inputInternalState.CurrentHorizontalInput, _inputSettings.minRangeX, _inputSettings.maxRangeX);
            var newX = Mathf.Lerp(position.x, clampedX, _inputConstants.Damping * Time.deltaTime);

            _transformSettings._targetTransform.position = new Vector3(newX, position.y, position.z);
        }


        #endregion
    }
}
