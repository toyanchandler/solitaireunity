using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [SerializeField] private bool canInteract = true;
        
        [SerializeField] private MonoBehaviour[] actionComponents = Array.Empty<MonoBehaviour>();

        [ShowInInspector, ReadOnly] private IInteractableAction[] actions = Array.Empty<IInteractableAction>();
        
        [SerializeField] private InteractableData interactableData;

        #endregion
        
        #region Private Fields
        
        private bool isInitialized;

        #endregion

        #region Public Properties

        public bool CanInteract => canInteract;

        #endregion

        #region Private Methods

        private void EnsureInitialized()
        {
            if (isInitialized) return;

            actions = GlobalProviderGuard.BuildActionCache<IInteractableAction>(this, actionComponents);
            isInitialized = true;
        }


        #endregion

        #region Unity Methods

        private void Awake()
        {
            EnsureInitialized();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            actionComponents = GlobalProviderGuard.CollectChildActions<IInteractableAction>(this);
        }
#endif

        #endregion

        #region Public Methods

        public void Interact()
        {
            EnsureInitialized();

            if (!CanInteract || actions == null || actions.Length == 0) return;

            EventManager.InteractableEvents.Interact?.Invoke(interactableData);

            foreach (var action in actions)
            {
                action.OnInteract();
            }
            canInteract = false;
        }


        #endregion
    }

    #region Data Structures

    [Serializable]
    public struct InteractableData
    {
        public float Amount;
    }

    #endregion
}
