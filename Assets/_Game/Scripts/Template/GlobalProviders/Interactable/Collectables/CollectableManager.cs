using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.UI.Buttons;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Collectables
{
    public class CollectableManager : MonoBehaviour
    {
        [SerializeField]
        private CollectableValuesSO collectableValuesSO;

        private void OnEnable()
        {
            EventManager.CollectableEvents.Collect += HandleCollectEvent;
            EventManager.CurrencySystem.TryToBuy += HandleTryToBuyEvent;
            EventManager.CurrencySystem.MultiplyRewardedAd += HandleMultiplyRewardedAdEvent;
        }

        private void OnDisable()
        {
            EventManager.CollectableEvents.Collect -= HandleCollectEvent;
            EventManager.CurrencySystem.TryToBuy -= HandleTryToBuyEvent;
            EventManager.CurrencySystem.MultiplyRewardedAd -= HandleMultiplyRewardedAdEvent;
        }

        private void HandleTryToBuyEvent(int cost, CollectableType collectableType, UnityAction<bool> callback)
        {
            if (collectableValuesSO.GetValue(collectableType) >= cost)
            {
                collectableValuesSO.SpendValue(CollectableType.Coin, cost);
                callback?.Invoke(true);
                EventManager.CurrencySystem.CollectableSpent?.Invoke(collectableType);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        private void HandleMultiplyRewardedAdEvent(MultiplyRewardedAdData multiplyRewardedAdData)
        {
            collectableValuesSO.MultiplyValue(multiplyRewardedAdData.CollectableType, multiplyRewardedAdData.MultiplyCount);
            EventManager.CurrencySystem.CollectableSpent?.Invoke(multiplyRewardedAdData.CollectableType);

        }

        private void HandleCollectEvent(CollectableData collectableData)
        {
            collectableValuesSO.AddValue(collectableData.CollectableType, collectableData.Amount);
        }
    }
}
