using System;
using System.Collections.Generic;
using _Game.Scripts.Helper.Services;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class ColorCycleProvider : MonoBehaviour
    {
        #region Private Variables

        private Material material;

        [SerializeField] private List<Color> colorCycle;
        [SerializeField] private float durationPerTransition = 0.15f;
        [SerializeField] private bool IsLoop = false;

        private Sequence colorSequence;

        private Coroutine colorCycleCoroutine;
        private CoroutineService coroutineService;

        private Color initialColor;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
            initialColor = material.color;
            coroutineService = new CoroutineService(this);
        }

        private void OnDisable()
        {
            colorSequence.Kill();
        }

        private void OnDestroy()
        {
            coroutineService.Stop(colorCycleCoroutine);
            colorSequence.Kill();
        }

        #endregion

        #region Protected Methods

        protected virtual void StartColorCycle() => colorCycleCoroutine = coroutineService.StartDelayedRoutine(InitColorCycle, 0f);

        protected virtual void StopColorCycle() => coroutineService.Stop(colorCycleCoroutine);

        #endregion

        #region Private Methods

        private void InitColorCycle()
        {
            material.DOColor(colorCycle[0], durationPerTransition / colorCycle.Count)
                .SetEase(Ease.Linear)
                .OnComplete(ColorCycleBehaviour);
        }

        private void ColorCycleBehaviour()
        {
            colorSequence?.Kill();

            colorSequence = DOTween.Sequence();

            for (int i = 0; i < colorCycle.Count; i++)
            {
                Color endColor = colorCycle[(i + 1) % colorCycle.Count];
                float duration = durationPerTransition / colorCycle.Count;
                colorSequence.Append(material.DOColor(endColor, duration).SetEase(Ease.Linear));
            }

            // Smooth transition back to the initial color
            float returnDuration = durationPerTransition / colorCycle.Count;
            colorSequence.Append(material.DOColor(initialColor, returnDuration).SetEase(Ease.Linear));

            colorSequence.OnComplete(() => {
                if (!IsLoop)
                {
                    colorSequence.Kill();
                }
            });

            if (IsLoop)
            {
                colorSequence.SetLoops(-1);
            }
        }


        #endregion
    }
}
