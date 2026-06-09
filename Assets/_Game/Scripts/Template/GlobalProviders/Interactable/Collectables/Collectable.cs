using System;
using _Game.Scripts.Managers.Core;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Collectables
{
    public sealed class Collectable : MonoBehaviour, IInteractableAction
    {
        #region Serialized Fields
        
        [SerializeField] private CollectableData collectableData;
        
        #endregion

        #region Private Variables
        private bool CanCollect { get; set; } = true;

        #endregion

        #region Public Methods

        public void OnInteract()
        {
            if (!CanCollect) return;
            
            EventManager.CollectableEvents.Collect?.Invoke(
                collectableData.CreateRuntimeData(gameObject, transform.position));
        }

        #endregion
    }
    
    [Serializable]
    public struct CollectableData
    {
        [SerializeField] private GameObject Collectable;

        [SerializeField] private CollectableType Type;

        [SerializeField] private int ScoreAmount;

        [HideInInspector]
        [SerializeField] private Vector3 collectedPosition;

        public GameObject CollectableGO => Collectable;
        public CollectableType CollectableType => Type;
        public int Amount => ScoreAmount;
        public Vector3 CollectedPosition => collectedPosition;

        public CollectableData CreateRuntimeData(GameObject runtimeCollectableGO, Vector3 runtimeCollectedPosition)
        {
            return new CollectableData
            {
                Collectable = runtimeCollectableGO,
                Type = Type,
                ScoreAmount = ScoreAmount,
                collectedPosition = runtimeCollectedPosition
            };
        }
    }
    
    [Serializable]
    public enum CollectableType
    {
        Coin,
        Gem,
    }
}
