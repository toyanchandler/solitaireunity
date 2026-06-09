using System.Collections.Generic;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Predefined
{
    [CreateAssetMenu(fileName = "IconProviderSO", menuName = "ThisGame/IconProviderSO", order = 1)]
    public class IconProviderSO : SerializedScriptableObject
    {
        [OdinSerialize] private Dictionary<CollectableType, Sprite> collectableIconDict;
        [OdinSerialize] private Dictionary<UpgradeType, Sprite> upgradeIconDict;
        
        public Sprite GetCollectableIcon(CollectableType collectableType)
        {
            if (collectableIconDict.TryGetValue(collectableType, out Sprite icon))
            {
                return icon;
            }
            else
            {
                Debug.LogWarning("Icon not found for the given CollectableType.");
                return null;
            }
        }
        
        public Sprite GetUpgradeIcon(UpgradeType upgradeType)
        {
            if (upgradeIconDict.TryGetValue(upgradeType, out Sprite icon))
            {
                return icon;
            }
            else
            {
                Debug.LogWarning("Icon not found for the given UpgradeType.");
                return null;
            }
        }
    }
}
