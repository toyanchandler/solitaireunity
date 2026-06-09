using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using UnityEngine;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public class RunnerObjectTranslating : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] public float _speed;
        [SerializeField] private Transform _transformToTranslate;
        
        #endregion
        
        #region Private Variables

        private CoroutineService _coroutineService;
        private Coroutine _translateCoroutine;

        #endregion
        
        #region Unity Methods

        protected virtual void Awake()
        {
            _coroutineService = new CoroutineService(this);
            
            if(_transformToTranslate == null)
                _transformToTranslate = transform;
        }

        private void OnEnable() => SubscribeEvents();

        private void OnDisable()
        {
            UnsubscribeEvents();
            _coroutineService.Stop(_translateCoroutine);
        }

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

        protected virtual void StartTranslate() => _translateCoroutine = _coroutineService.StartUpdateRoutine(Translate, () => true);

        protected virtual void StopTranslate() => _coroutineService.Stop(_translateCoroutine);

        protected virtual void Translate() => _transformToTranslate.Translate(Vector3.forward * (Time.deltaTime * _speed), Space.World);

        #endregion
    }
}
