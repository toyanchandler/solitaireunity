using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using _Game.Scripts.UI.Buttons;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class UpgradeSystem
        {
            public static UnityAction<UpgradeType,UpgradeButtonStruct> UpgradeButtonClicked; //TODO : UpgradeButton Struct.
           
            public static UnityAction<UpgradeType> CharacterUpgraded;
        }

        public static class CurrencySystem
        {
            public static UnityAction<int,CollectableType,UnityAction<bool>> TryToBuy;
            public static UnityAction<CollectableType> CollectableSpent;
            public static UnityAction<MultiplyRewardedAdData> MultiplyRewardedAd;
        }
     
    }
}
