using System.Collections;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Template.GlobalProviders.Combat;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public sealed class GateInteractable : BaseGateInteractable, IDamageableAction
    {
        #region Public Variables

        public TextMeshPro textMeshPro;

        #endregion

        #region Serialized Fields

        [SerializeField] private GateView gateView;
        [SerializeField] private GateAnimator gateAnimator;
        [SerializeField] private GateDamageHandler gateDamageHandler;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ResolveComponents();
            InitiateSetGateData();
        }

        private void Reset()
        {
            ResolveComponents();
        }

        private void OnValidate()
        {
            ResolveComponents(false);
            RefreshGateAppearance();
        }

        #endregion

        #region Inherited Methods

        protected override void OnGateInteraction()
        {
            TDebug.Log(gateInteractableData.Amount + " " + gateInteractableData.mathType + " Gate Interacted");
            CleanupFluxyContainer();
            
            Destroy(gameObject);
        }

        #endregion

        #region Private Methods

        #region Initialization

        private void InitiateSetGateData()
        {
            if (!isActiveAndEnabled) return;
            StartCoroutine(DeferRefreshGateAppearance());
        }

        private void InitiateSetColorData()
        {
            InitiateSetGateData();
        }

        private IEnumerator DeferRefreshGateAppearance()
        {
            yield return null;
            RefreshGateAppearance();
        }

        private void ResolveComponents(bool createMissing = true)
        {
            gateView = ResolveComponent(gateView, createMissing);
            gateAnimator = ResolveComponent(gateAnimator, createMissing);
            gateDamageHandler = ResolveComponent(gateDamageHandler, createMissing);
            gateView?.Configure(textMeshPro, GetComponent<MeshRenderer>());
        }

        private T ResolveComponent<T>(T component, bool createMissing) where T : Component
        {
            if (component != null) return component;

            component = GetComponent<T>();
            return component != null || !createMissing ? component : gameObject.AddComponent<T>();
        }

        #endregion

        #region Gate Data

        private void RefreshGateAppearance()
        {
            if (gateView == null) return;
            gateView.Refresh(gateInteractableData);
        }

        #endregion

        [ShowIf("@isGuardedGate")]
        [Button]
        private void GuardedGateOpenTask()
        {
            ApplyGateDamage();
        }

        #region Damage Handling

        private void ApplyGateDamage()
        {
            ResolveComponents();
            gateDamageHandler.ApplyDamage(
                ref gateInteractableData,
                ref guardedGateData,
                isGuardedGate,
                () => gateAnimator.OpenDoors(guardedGateData),
                canInteract => CanInteract = canInteract);
            RefreshGateAppearance();
        }

        private void CleanupFluxyContainer()
        {
            if (Container == null) return;

            Container.targets = System.Array.Empty<Fluxy.FluxyTarget>();
            Destroy(Container.gameObject);
        }

        #endregion

        #endregion

        #region IDamageableAction Implementation

        public void Initialize(DamageableObject damageableObject)
        {
            // Implementation
        }

        public void TakeDamage(float damage)
        {
            ApplyGateDamage();
        }

        public void HealthChanged(float currentHealth)
        {
            // Implementation
        }

        public void Death()
        {
            // Implementation
        }

        #endregion
    }
}
