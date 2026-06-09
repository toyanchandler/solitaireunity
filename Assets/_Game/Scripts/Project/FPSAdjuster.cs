using System;
using _Game.Scripts.Helper.Services;
using UnityEngine;

namespace _Game.Scripts.Project
{
    public class FPSAdjuster : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableAutoQualityAdjustment = true;  // Enable or disable automatic quality adjustments

        #endregion

        #region Private Variables

        private CoroutineService _coroutineService;
        private Coroutine _adjustQualityCoroutine;

        #endregion

        #region Unity Methods

        public void Awake()
        {
            InitializeSettings();
            SetVSync(false);
            
            _coroutineService = new CoroutineService(this);
        }

        private void OnEnable()
        {
            StartAutoQualityAdjustment();
        }

        private void OnDisable()
        {
            StopAutoQualityAdjustment();
        }

        #endregion

        #region Private Methods

        private void StartAutoQualityAdjustment()
        {
            _adjustQualityCoroutine = _coroutineService.StartUpdateRoutine(AutoAdjustQuality, () => enableAutoQualityAdjustment);
        }
        
        private void StopAutoQualityAdjustment()
        {
            _coroutineService.Stop(_adjustQualityCoroutine);
        }
        
        private void InitializeSettings()
        {
            // Sets the target frame rate
            SetTargetFrameRate(targetFrameRate);

            // Adjusts physics timestep based on target frame rate
            //Time.fixedDeltaTime = 1f / targetFrameRate;
        }
        
        private void SetTargetFrameRate(int frameRate)
        {
            Application.targetFrameRate = frameRate;
        }
        
        private void SetVSync(bool vSync)
        {
            QualitySettings.vSyncCount = vSync ? 1 : 0;
        }

        /// <summary>
        /// Automatically adjust quality settings based on current FPS.
        /// </summary>
        private void AutoAdjustQuality()
        {
            float currentFPS = 1.0f / Time.deltaTime;

            // Decrease quality if FPS is too low
            if (currentFPS < targetFrameRate - 5)
            {
                QualitySettings.DecreaseLevel();
            }
            // Increase quality if FPS is higher than target
            else if (currentFPS > targetFrameRate + 5)
            {
                QualitySettings.IncreaseLevel();
            }
        }

        #endregion
    }
}