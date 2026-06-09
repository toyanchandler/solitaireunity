using System;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class PunchScaleProvider : MonoBehaviour
    {
        #region Private Variables

        private Tween _tween;
        
        private Vector3 originalScale;

        private Sequence localSequence;
        
        #endregion

        #region Inspector Variables

        [SerializeField] private float duration = 0.25f;

        [SerializeField] private float desiredScale;

        #endregion

        #region Unity Events

        private void Awake() => originalScale = transform.localScale;

        private void OnDestroy() => localSequence.Kill();

        #endregion

        #region Protected Methods

        protected virtual void PunchScale()
        {
            localSequence?.Kill();
            
            localSequence = DOTween.Sequence();
            
            Vector3 targetScale = originalScale + (Vector3.one * desiredScale); // Desired scale
            localSequence.Append(transform.DOScale(targetScale, duration).SetEase(Ease.OutQuad)); // Scale up
            localSequence.Append(transform.DOScale(originalScale, duration).SetEase(Ease.InQuad)); // Scale down
    
            localSequence.OnComplete(() =>
            {
                transform.localScale = originalScale;
            });
    
            localSequence.Play();
        }

        #endregion
        
    }
}