using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public class RunnerTouchInput : MonoBehaviour
    {
        #region Private Variables

        private CoroutineService _coroutineService;
        
        #endregion

        #region Input Variables

        [SerializeField] private InputDataContainer.TransformSettings _transformSettings;
        [SerializeField] private InputDataContainer.InputSettings _inputSettings;
        [SerializeField] private InputDataContainer.InputConstants _inputConstants;
        private InputDataContainer.InputInternalState _inputInternalState;

        private Vector3 currentMousePosition;
        private Vector3 mouseDelta;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _coroutineService = new CoroutineService(this);
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            StopInput();
        }

        #endregion

        #region Subscribe/Unsubscribe Events

        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelLoaded += GetDefaults;
            EventManager.InGameEvents.LevelStart += StartInput;
            EventManager.InGameEvents.LevelSuccess += StopInput;
            EventManager.InGameEvents.LevelFail += StopInput;
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelLoaded -= GetDefaults;
            EventManager.InGameEvents.LevelStart -= StartInput;
            EventManager.InGameEvents.LevelSuccess -= StopInput;
            EventManager.InGameEvents.LevelFail -= StopInput;
        }

        #endregion

        #region Private Methods

        private void GetDefaults(GameObject go)
        {
            GetTransformDefaults();
        }
        
        private void GetTransformDefaults()
        {
            if (_transformSettings._targetTransform == null)
            {
                _transformSettings._targetTransform = transform;
            }
        }
        
        private void StartInput()
        {
            _coroutineService.StartLateUpdateRoutine(InputUpdate, () => true);
        }

        private void StopInput()
        {
            _coroutineService.StopAll();
        }

        private void InputUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                currentMousePosition = Input.mousePosition;
                
                _inputInternalState.Sensitivity = _inputSettings.BaseSensitivity*(Screen.width / 1080f);
                mouseDelta = Input.mousePosition - new Vector3(Input.mousePosition.x - Input.GetAxis("Mouse X"),
                    Input.mousePosition.y,
                    Input.mousePosition.z);

                var horizontalInput = mouseDelta.x*_inputInternalState.Sensitivity;
                HandleHorizontalInput(horizontalInput);
            }
            else
            {
                _inputInternalState.CurrentHorizontalInput = 0;
                mouseDelta = Vector3.zero;
            }
        }

        private void HandleHorizontalInput(float horizontalInput)
        {
            _inputInternalState.LastHorizontalInput = horizontalInput;
            HandleHorizontalInputWithDamping(_inputInternalState.LastHorizontalInput);
        }

        private void HandleHorizontalInputWithDamping(float newHorizontalInput)
        {
            _inputInternalState.CurrentHorizontalInput = Mathf.Lerp(_inputInternalState.CurrentHorizontalInput, newHorizontalInput, _inputConstants.InputLag);

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
