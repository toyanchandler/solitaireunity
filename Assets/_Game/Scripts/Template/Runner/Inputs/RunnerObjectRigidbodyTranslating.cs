using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public class RunnerObjectRigidbodyTranslating : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private float _speed;
        [SerializeField] private Rigidbody _rigidbodyToTranslate; // Added Rigidbody to translate using physics

        #endregion

        #region Private Variables

        private CoroutineService _coroutineService;
        private Coroutine _translateCoroutine;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _coroutineService = new CoroutineService(this);

            if(_rigidbodyToTranslate == null)
                _rigidbodyToTranslate = GetComponent<Rigidbody>();
        }

        private void OnEnable() => SubscribeEvents();

        private void OnDisable() => UnsubscribeEvents();

        #endregion

        #region Event Subscribing/Unsubscribing

        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart += StartTranslate;
            EventManager.InGameEvents.LevelSuccess += StopTranslate;
            EventManager.InGameEvents.LevelFail += StopTranslate;
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart -= StartTranslate;
            EventManager.InGameEvents.LevelSuccess -= StopTranslate;
            EventManager.InGameEvents.LevelFail -= StopTranslate;
        }

        #endregion

        #region Private Methods

        private void StartTranslate() => _translateCoroutine = _coroutineService.StartFixedUpdateRoutine(Translate, () => true); // Changed to FixedUpdateRoutine for physics related tasks

        private void StopTranslate() => _coroutineService.Stop(_translateCoroutine);

        private void Translate()
        {
            var velocity = _rigidbodyToTranslate.linearVelocity;
            Vector3 newVelocity = new Vector3(velocity.x, velocity.y, _speed);
            velocity = newVelocity;
            _rigidbodyToTranslate.linearVelocity = velocity;
        }

        #endregion
    }
}
