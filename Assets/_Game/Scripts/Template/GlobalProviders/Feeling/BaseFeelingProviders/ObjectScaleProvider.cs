using _Game.Scripts.Helper.Services;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class ObjectScaleProvider : MonoBehaviour
    {
        #region Private Variables

        [SerializeField] private Vector3 startScale = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 endScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool isRandomizedStartScale;
        [SerializeField] private bool oneTimeOnly = false;

        private Tween scaleTween;

        private CoroutineService coroutineService;

        private Coroutine scaleCoroutine;

        #endregion

        #region Unity Methods

        private void Awake() => coroutineService = new CoroutineService(this);

        private void OnDisable()
        {
            scaleTween.Kill();
        }

        #endregion

        #region Protected Methods

        protected virtual void StartScaleCoroutine() =>
            scaleCoroutine = coroutineService.StartDelayedRoutine(ObjectScaling, isRandomizedStartScale
                ? Random.Range(0f, 0.5f) : 0f);

        protected virtual void StopScaleCoroutine() => coroutineService.Stop(scaleCoroutine);

        #endregion

        #region Private Methods

        /// <summary>
        /// ObjectScaling scales the object from startScale to endScale
        /// </summary>
        private void ObjectScaling()
        {
            transform.localScale = startScale;  // Reset the scale to the start value

            scaleTween = transform.DOScale(endScale, duration)
                .SetEase(Ease.Linear)
                .SetLoops(oneTimeOnly ? 1 : -1, LoopType.Yoyo); // Yoyo loop type for scaling back and forth

            if (oneTimeOnly)
            {
                scaleTween.OnComplete(() => scaleTween.Kill());
            }
        }

        #endregion
    }
}
