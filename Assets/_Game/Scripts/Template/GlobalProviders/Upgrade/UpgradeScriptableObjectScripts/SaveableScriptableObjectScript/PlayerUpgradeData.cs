using System.Collections.Generic;
using _Game.Scripts.ScriptableObjects.RunTime;
using _Game.Scripts.ScriptableObjects.Saveable;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Upgrade
{
    [CreateAssetMenu(fileName = "PlayerUpgradeData", menuName = "ThisGame/Upgrade/PlayerUpgradeData", order = 1)]
    public class PlayerUpgradeData : PersistentSaveManager<PlayerUpgradeData>, IResettable
    {
        [OdinSerialize] private Dictionary<UpgradeType, int> upgradeLevelDict = new Dictionary<UpgradeType, int>();

        public int GetUpgradeLevel(UpgradeType upgradeType)
        {
            if (upgradeLevelDict.TryGetValue(upgradeType, out int level))
            {
                return level;
            }

            upgradeLevelDict.Add(upgradeType, 1);
            return 1;
        }

        public void SetUpgradeLevel(UpgradeType upgradeType, int level)
        {
            if (upgradeLevelDict.ContainsKey(upgradeType))
            {
                upgradeLevelDict[upgradeType] = level;
            }
            else
            {
                upgradeLevelDict.Add(upgradeType, level);
            }
        }

        public void IncreaseUpgradeLevel(UpgradeType upgradeType, int increaseAmount = 1)
        {
            if (upgradeLevelDict.ContainsKey(upgradeType))
            {
                upgradeLevelDict[upgradeType] += increaseAmount;
            }
            else
            {
                upgradeLevelDict.Add(upgradeType, increaseAmount);
            }
        }

        public int GetCurrentLevelIndex(UpgradeType upgradeType)
        {
            return upgradeLevelDict.TryGetValue(upgradeType, out int level) ? level : 1;
        }

        public IReadOnlyDictionary<UpgradeType, int> GetUpgradeLevelDict()
        {
            return upgradeLevelDict;
        }
    }
}
