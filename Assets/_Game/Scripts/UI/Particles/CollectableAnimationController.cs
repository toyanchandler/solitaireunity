using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Predefined;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using AssetKits.ParticleImage;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.UI.Particles
{
    public class CollectableAnimationController : SerializedMonoBehaviour
    {
        #region Inspector Fields
        
        [SerializeField] private RectTransformData _rectTransformData;
        
        [SerializeField] private UIAnimationPrefabSO _uiAnimationPrefabSo;
        
        #endregion

        #region Private Variables

        private RectTransform _selfRectTransform;
        
        private Camera _mainCamera;
        
        private Sequence _sequence;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (_selfRectTransform == null)
                _selfRectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() => EventManager.CollectableEvents.Collect += AnimateCoroutine;

        private void OnDisable() => EventManager.CollectableEvents.Collect -= AnimateCoroutine;

        #endregion

        #region Private Methods

        private ParticleImage CreateParticleImage(CollectableData data)
        {
            var _viewTransform = _rectTransformData._viewTransform;
            
            if (_viewTransform == null) return null;
            
            var _uiPrefab = _uiAnimationPrefabSo.GetUIPrefab(data.CollectableType);
            
            var particleImage = Instantiate(_uiPrefab, _viewTransform.position, Quaternion.identity,
                _viewTransform).GetComponent<ParticleImage>();
            
            particleImage.attractorTarget = GetComponent<RectTransform>();
            
            return particleImage;
        }
        
        private void AnimateCoroutine(CollectableData collectableData)
        {
            var currentParticleImage = CreateParticleImage(collectableData);
            
            if (currentParticleImage == null) return;
            
            currentParticleImage.onAnyParticleFinished.AddListener(PunchScaleTarget);
            
            currentParticleImage.onLastParticleFinished.AddListener(() =>
            {
                EventManager.CollectableEvents.UICollectAnimation?.Invoke(collectableData);
                
                Destroy(currentParticleImage.gameObject);
            });
        }
        
        private void PunchScaleTarget()
        {
            var _targetTransform = _rectTransformData._targetTransform;
            
            _sequence = DOTween.Sequence();
            
            _sequence.Append(_targetTransform.DOScale(1.5f, 0.05f));
            _sequence.Append(_targetTransform.DOScale(1f, 0.05f)).SetDelay(0.01f);
        }

        #endregion
    }

    [Serializable]
    public struct RectTransformData
    {
        public RectTransform _viewTransform;
        
        public Transform _targetTransform;
    }
}
