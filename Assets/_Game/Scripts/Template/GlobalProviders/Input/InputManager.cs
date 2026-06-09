using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Input
{
    public sealed class InputManager : InputProvider
    {
        [SerializeField] private float _baseSensitivity = 30f;
        [SerializeField] private float _inputLag = 0.009f;

        private float _currentHorizontalInput;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.InGameEvents.LevelStart += ResetInput;
            EventManager.InGameEvents.LevelSuccess += ResetInput;
            EventManager.InGameEvents.LevelFail += ResetInput;
            EventManager.ClickableEvents.ClickHold += PublishMovementInput;
            EventManager.ClickableEvents.ClickUp += ResetInput;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.InGameEvents.LevelStart -= ResetInput;
            EventManager.InGameEvents.LevelSuccess -= ResetInput;
            EventManager.InGameEvents.LevelFail -= ResetInput;
            EventManager.ClickableEvents.ClickHold -= PublishMovementInput;
            EventManager.ClickableEvents.ClickUp -= ResetInput;
            ResetInput();
        }

        protected override void OnClickDown(ClickData clickData)
        {
        }

        protected override void OnClickHold(ClickData clickData)
        {
        }

        protected override void OnClickUp(ClickData clickData)
        {
        }

        private void PublishMovementInput(ClickData clickData)
        {
            float sensitivity = _baseSensitivity * (Screen.width / 1080f);
            float horizontalInput = UnityEngine.Input.GetAxis("Mouse X") * sensitivity;
            _currentHorizontalInput = Mathf.Lerp(_currentHorizontalInput, horizontalInput, _inputLag);

            EventManager.InputEvents.HorizontalInputChanged?.Invoke(_currentHorizontalInput);
        }

        private void ResetInput()
        {
            _currentHorizontalInput = 0f;
            EventManager.InputEvents.HorizontalInputChanged?.Invoke(0f);
        }

        private void ResetInput(ClickData clickData)
        {
            ResetInput();
        }
    }
}
