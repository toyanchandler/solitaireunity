using System;
using System.Collections.Generic;
using _Game.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Fluxy;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public abstract class BaseGateInteractable : MonoBehaviour, IInteractableAction
    {
        #region Public Variables

        [OnValueChanged("InitiateSetGateData")] [OnValueChanged("InitiateSetColorData")]
        public GateInteractableData gateInteractableData;

        [SerializeField] public bool CanInteract;
        [SerializeField] public bool isGuardedGate;

        [ShowIf("@isGuardedGate")]
        public GuardedGateData guardedGateData;

        public FluxyContainer Container;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            SetThisObject();
            Subscribe();
        }

        private void OnDisable()
        {
            UnSubscribe();
        }

        #endregion

        #region Private Methods

        private void Subscribe()
        {
            EventManager.ShootableEvents.FluidOnShoot += InitiateFluxyData;
        }

        private void UnSubscribe()
        {
            EventManager.ShootableEvents.FluidOnShoot -= InitiateFluxyData;
        }

        private void SetThisObject()
        {
            if (gateInteractableData.InteractableGameObject != null) return;
            gateInteractableData.InteractableGameObject = gameObject;
        }

        #endregion

        #region Public Methods
        public void OnInteract()
        {
            Interact();
        }

        #endregion

        #region Abstract Methods
        protected abstract void OnGateInteraction();

        #endregion

        #region Private Methods
        private void Interact()
        {
            if (!CanInteract) return;

            EventManager.InteractableEvents.GateInteract?.Invoke(new GateInteractableData
            {
                InteractableGameObject = gateInteractableData.InteractableGameObject,
                Amount = gateInteractableData.Amount,
                mathType = gateInteractableData.mathType,
                GateType = gateInteractableData.GateType
            });

            CanInteract = false;

            OnGateInteraction();
        }
        
        private void InitiateFluxyData(FluxyTarget target)
        {
            if (Container == null) return;

            List<FluxyTarget> targets = new List<FluxyTarget>();
            
            targets.Add(target);

            Container.targets = targets.ToArray();
        }

        #endregion
    }

    [Serializable]
    public struct GateInteractableData
    {
        public GameObject InteractableGameObject;

        public int Amount;

        public MathType mathType;

        public GateType GateType;
    }

    [Serializable]
    public struct GuardedGateData
    {
        public int CurrentAmount;

        public int MaxAmount;

        public List<GameObject> DoorCovers;

        public float coverPositionOffset;
    }

    [EnumToggleButtons]
    public enum MathType
    {
        Add,
        Subtract,
        Divide,
        Multiply
    }

    [EnumToggleButtons]
    public enum GateType
    {
        FireRate,
        Damage,
        Range,
        Speed
    }
}
