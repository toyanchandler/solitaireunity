using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Helper.Services;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class ObjectRotateProvider : MonoBehaviour
    {
        #region Private Variables

        [SerializeField] private Vector3 rotationAngles = new Vector3(0f, 360f, 0f);
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool useWorldSpace = true;
        [SerializeField] private bool isRandomizedStartRotation;
        [SerializeField] private LoopType loopType = LoopType.Incremental;
        [SerializeField] private bool oneTimeOnly = false; 
        
        private Tween rotationTween;
        
        private CoroutineService coroutineService;
        
        private Coroutine rotationCoroutine;
        
        #endregion

        #region Unity Methods

        private void Awake() => coroutineService = new CoroutineService(this);

        private void OnDestroy()
        {
            StopRotateCoroutine();
            rotationTween.Kill();
        }

        #endregion

        #region Protected Methods

        protected virtual void StartRotateCoroutine() =>
            rotationCoroutine = coroutineService.StartDelayedRoutine(ObjectRotation, isRandomizedStartRotation 
                ? Random.Range(0f, 0.5f) : 0f);
        
        protected virtual void StopRotateCoroutine() => coroutineService.StopAll();
        
        #endregion
        
        #region Private Methods
        
        private void ObjectRotation()
        {
            rotationTween = transform.DORotate(
                    transform.eulerAngles + rotationAngles, 
                    duration, 
                    useWorldSpace ? RotateMode.WorldAxisAdd : RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.Linear)
                .SetLoops(oneTimeOnly ? 1 : -1, loopType);  // Updated set loop based on oneTimeOnly

            if (oneTimeOnly)
            {
                rotationTween.OnComplete(() => rotationTween.Kill());  // Stop the tween when it completes
            }
        }
        
        #endregion
    }
}
