using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public class ChangeScaleOnGateInteract : MonoBehaviour
    {
        #region Private Variables

        private Vector3 _previousScale;

        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _minScale = 0.5f;
        [SerializeField] private float _maxScale = 2.0f;
        [SerializeField] private float _maxAmount = 10.0f;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            _previousScale = transform.localScale;
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        #endregion

        #region Shared Methods

        private void Subscribe()
        {
            EventManager.InteractableEvents.GateInteract += OnGateInteract;
        }

        private void Unsubscribe()
        {
            EventManager.InteractableEvents.GateInteract -= OnGateInteract;
        }

        #endregion

        #region Private Methods

        private void OnGateInteract(GateInteractableData data)
        {
            float targetScale = CalculateTargetScale(data);
            transform.DOScale(targetScale, _duration).SetEase(Ease.OutBounce)
                .OnComplete(() => { _previousScale = transform.localScale; });
        }


        private float CalculateTargetScale(GateInteractableData data)
        {
            float deltaScale = 0f;

            // Calculate the mapped delta scale based on data.Amount
            deltaScale = Mathf.InverseLerp(_minScale, _maxScale, data.Amount / _maxAmount);

            float targetScale = _previousScale.x;

            switch (data.mathType)
            {
                case MathType.Subtract:
                    targetScale -= deltaScale;  // Here, you can replace deltaScale with your own calculation if needed.
                    break;
                case MathType.Add:
                    targetScale += deltaScale;
                    break;
                case MathType.Divide:
                    targetScale /= deltaScale;  // Again, replace deltaScale with your own calculation if needed.
                    break;
                case MathType.Multiply:
                    targetScale *= deltaScale;  // Replace deltaScale with your own calculation if needed.
                    break;
            }
            
            return targetScale;
        }

        #endregion
    }
}
