using System;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Stacking
{
    public abstract class BaseStackable : MonoBehaviour, IInteractableAction
    {
        #region Inspector Variables

        [SerializeField] private bool canStack = true;

        [SerializeField] private StackableData stackableDataStruct;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            EventManager.StackableEvents.Unstack += CallUnstack;
        }

        private void OnDisable()
        {
            EventManager.StackableEvents.Unstack -= CallUnstack;   
        }

        #endregion

        #region Private Methods
        
        private void CallStack()
        {
            if (!canStack) return;
    
            EventManager.StackableEvents.Stack?.Invoke(stackableDataStruct.CreateRuntimeData(gameObject));
    
            OnStacking(gameObject);
        }

        private void CallUnstack(StackableData _stackableData)
        {
            if (!canStack) return;
            
            OnUnstacking(gameObject);
        }

        #endregion
        
        #region Public Methods

        public void OnInteract()
        {
            CallStack();
        }
        
        #endregion

        #region Abstract Methods

        protected abstract void OnStacking(GameObject stackable);
        protected abstract void OnUnstacking(GameObject stackable);

        #endregion
    }
    
    [Serializable]
    public struct StackableData
    {
        [SerializeField] private GameObject StackableObject;

        [SerializeField] private int StackableObjectAmount;

        [SerializeField] private StackableType stackableType;

        public GameObject Object => StackableObject;
        public int Amount => StackableObjectAmount;
        public StackableType Type => stackableType;

        public StackableData CreateRuntimeData(GameObject runtimeStackableObject)
        {
            return new StackableData
            {
                StackableObject = runtimeStackableObject,
                StackableObjectAmount = StackableObjectAmount,
                stackableType = stackableType
            };
        }

        public enum StackableType
        {
            StackableType1,
            StackableType2,
            StackableType3,
        }
    }
}
