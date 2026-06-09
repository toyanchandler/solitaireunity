using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using EventManager = _Game.Scripts.Managers.Core.EventManager;

namespace _Game.Scripts.Template.GlobalProviders.Visual
{
    [RequireComponent(typeof(TrailRenderer))]
    public class VisualTrailProvider : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private List<VisualTrailProviderData> _dataList;
        [SerializeField] private float transitionDuration;
        

        #endregion

        #region Private Variables

        private int dataCount;
        
        private int currentDataIndex;

        private int defaultTrailTime = 3;

        #endregion

        #region Unity Methods

        private void OnEnable() => SubscribeEvents();

        private void OnDisable() => UnsubscribeEvents();

        #endregion

        #region Event Subscribing/Unsubscribing

        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart += InitTrailRenderer;
            EventManager.InGameEvents.LevelStart += () =>
            {
                SetTime(defaultTrailTime);
            };
            
            EventManager.InGameEvents.LevelSuccess += () =>
            {
                SetTime(0);
            };
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelStart -= InitTrailRenderer;
            EventManager.InGameEvents.LevelStart -= () =>
            {
                SetTime(defaultTrailTime);
            };
            
            EventManager.InGameEvents.LevelSuccess -= () =>
            {
                SetTime(0);
            };
        }

        #endregion

        #region Private Methods

        private void SetTime(float time) => _trailRenderer.time = time;

        private void SetTrailRendererActive(bool active) => _trailRenderer.enabled = active;

        private void SetWidth(float width) => _trailRenderer.widthMultiplier = width;

        private void SetAlignment(LineAlignment alignment) => _trailRenderer.alignment = alignment;
        
        private void SetRandomColor() => _trailRenderer.startColor = _trailRenderer.endColor = UnityEngine.Random.ColorHSV();
        
        private void SetColor(Color color) => _trailRenderer.startColor = _trailRenderer.endColor = color;

        private void StopTrailRenderer() => _trailRenderer.enabled = false;
        
        private void InitTrailRenderer()
        {
            // An example: Apply the first data set initially
            var initialData = _dataList[0];
            SetTime(initialData.Time);
            SetTrailRendererActive(initialData.Active);
            SetWidth(initialData.Width);
            SetAlignment(initialData.Alignment);
            SetColor(initialData.Color);

            dataCount = _dataList.Count;
        }
        

        [Button]
        private void TransitionToNextData()
        {
            currentDataIndex++;

            var nextData = _dataList[currentDataIndex % dataCount];
            
            DOTween.To(() => _trailRenderer.time, x => _trailRenderer.time = x, nextData.Time, transitionDuration);
            DOTween.To(() => _trailRenderer.widthMultiplier, x => _trailRenderer.widthMultiplier = x, nextData.Width, transitionDuration);
            SetColor(nextData.Color);
        }

        #endregion
    }

    [Serializable]
    public struct VisualTrailProviderData
    {
        public float Time;
        public bool Active;
        public float Width;
        public LineAlignment Alignment;
        public Color Color;
    }
}
