using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public sealed class CharacterMover : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _forwardSpeed = 4f;
        [SerializeField] private float _minRangeX = -3f;
        [SerializeField] private float _maxRangeX = 3f;
        [SerializeField] private float _damping = 3f;

        private CoroutineService _coroutineService;
        private Coroutine _moveCoroutine;
        private float _horizontalInput;

        private void Awake()
        {
            _coroutineService = new CoroutineService(this);

            if (_targetTransform == null)
            {
                _targetTransform = transform;
            }
        }

        private void OnEnable()
        {
            EventManager.InputEvents.HorizontalInputChanged += SetHorizontalInput;
            EventManager.InGameEvents.LevelStart += StartMove;
            EventManager.InGameEvents.LevelSuccess += StopMove;
            EventManager.InGameEvents.LevelFail += StopMove;
        }

        private void OnDisable()
        {
            EventManager.InputEvents.HorizontalInputChanged -= SetHorizontalInput;
            EventManager.InGameEvents.LevelStart -= StartMove;
            EventManager.InGameEvents.LevelSuccess -= StopMove;
            EventManager.InGameEvents.LevelFail -= StopMove;
            StopMove();
        }

        private void SetHorizontalInput(float horizontalInput)
        {
            _horizontalInput = horizontalInput;
        }

        private void StartMove()
        {
            StopMove();
            _moveCoroutine = _coroutineService.StartUpdateRoutine(Move, () => true);
        }

        private void StopMove()
        {
            _coroutineService?.Stop(_moveCoroutine);
            _moveCoroutine = null;
            _horizontalInput = 0f;
        }

        private void Move()
        {
            Vector3 position = _targetTransform.position;
            float clampedX = Mathf.Clamp(position.x + _horizontalInput, _minRangeX, _maxRangeX);
            float nextX = Mathf.Lerp(position.x, clampedX, _damping * Time.deltaTime);
            float nextZ = position.z + _forwardSpeed * Time.deltaTime;

            _targetTransform.position = new Vector3(nextX, position.y, nextZ);
        }
    }
}
