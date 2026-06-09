using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public class RandomScaleOnCoinCollect : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float minScale = 0.8f;
        [SerializeField] private float maxScale = 1.7f;

        private void OnEnable()
        {
            EventManager.CollectableEvents.Collect += HandleCollect;
        }

        private void OnDisable()
        {
            EventManager.CollectableEvents.Collect -= HandleCollect;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (maxScale < minScale)
            {
                maxScale = minScale;
            }
        }
#endif

        private void HandleCollect(CollectableData collectableData)
        {
            if (collectableData.CollectableType != CollectableType.Coin)
            {
                return;
            }

            Transform scaleTarget = targetTransform != null ? targetTransform : transform;
            float randomScale = Random.Range(minScale, maxScale);
            scaleTarget.localScale = Vector3.one * randomScale;
        }
    }
}
