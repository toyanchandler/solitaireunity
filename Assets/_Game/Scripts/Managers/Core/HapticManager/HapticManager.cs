using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Interactable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using _Game.Scripts.Template.GlobalProviders.Interactable.Stacking;
using Handler.Extensions;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace _Game.Scripts.Managers.Core.HapticManager
{
    public sealed class HapticManager : MonoBehaviour
    {
        #region Serialized Variables

        [SerializeField] private SettingsDataSO settingsData;
        
        [SerializeField] private float HapticIntensity = 1f;
        [SerializeField] private float HapticSharpness = 1f;

        #endregion
        
        #region Unity Lifecycle Methods

        private void Awake()
        {
            ValidateReferences();
        }
        
        private void OnEnable()
        {
            EventManager.InteractableEvents.Interact += OnInteract;
            EventManager.InteractableEvents.GateInteract += OnGateInteract;
            EventManager.ObstacleInteractableEvents.ObstacleInteract += OnObstacleInteract;
            EventManager.StackableEvents.Stack += OnStack;
            EventManager.StackableEvents.Unstack += OnUnstack;
            EventManager.HealthEvents.DamageableDeath += OnDamageableDeath;
            EventManager.InGameEvents.LevelSuccess += OnSuccess;
        }

        private void OnDisable()
        {
            EventManager.InteractableEvents.Interact -= OnInteract;
            EventManager.InteractableEvents.GateInteract -= OnGateInteract;
            EventManager.ObstacleInteractableEvents.ObstacleInteract -= OnObstacleInteract;
            EventManager.StackableEvents.Stack -= OnStack;
            EventManager.StackableEvents.Unstack -= OnUnstack;
            EventManager.HealthEvents.DamageableDeath -= OnDamageableDeath;
            EventManager.InGameEvents.LevelSuccess -= OnSuccess;
        }
        
        #endregion

        #region Public Methods

        public void Vibrate()
        {
            if (CanVibrate())
            {
                MMVibrationManager.Haptic(HapticTypes.LightImpact, false, true, this);
            }
        }

        #endregion

        #region Event Callbacks
        private void OnInteract(InteractableData _data)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }

        private void OnGateInteract(GateInteractableData _data)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }

        private void OnObstacleInteract(float value)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }

        private void OnStack(StackableData _data)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }

        private void OnUnstack(StackableData _data)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }

        private void OnDamageableDeath(DamageableData _data)
        {
            if (CanVibrate())
            {
                MMVibrationManager.TransientHaptic(HapticIntensity, HapticSharpness, true, this);
            }
        }
        
        private void OnSuccess()
        {
            if (CanVibrate())
            {
                MMVibrationManager.Haptic(HapticTypes.Success, false, true, this);
            }
        }
        
        #endregion

        private bool CanVibrate()
        {
            if (settingsData == null)
            {
                TDebug.LogWarning($"{nameof(HapticManager)} requires SettingsDataSO to resolve vibration state.");
                return false;
            }

            return settingsData.IsVibrationEnabled;
        }

        private void ValidateReferences()
        {
            if (settingsData == null)
            {
                TDebug.LogWarning($"{nameof(HapticManager)} requires SettingsDataSO.");
            }
        }
    }
}
