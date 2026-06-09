using System;
using System.Collections.Generic;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using _Game.Scripts.UI.Buttons;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Upgrade
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private List<UpgradableSO> upgradableSOs;
        [SerializeField] private PlayerUpgradeData playerUpgradeData;
        private readonly List<UpgradeType> _levelTemporaryUpgrades = new List<UpgradeType>();

        private void OnEnable()
        {
            SubscribeEvents();
        }
        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {
            EventManager.UpgradeSystem.UpgradeButtonClicked += OnUpgradeButtonClicked;
            EventManager.InGameEvents.LevelStart+= CacheUpgradesInitialValuesOnLevelStart;
            EventManager.InGameEvents.LevelSuccess += ResetUpgrades;
            //EventManager.InteractableEvents.GateInteract += HandleWithGateInteract;
        }

        private void UnsubscribeEvents()
        {
            EventManager.UpgradeSystem.UpgradeButtonClicked -= OnUpgradeButtonClicked;
            EventManager.InGameEvents.LevelStart-= CacheUpgradesInitialValuesOnLevelStart;
            EventManager.InGameEvents.LevelSuccess -= ResetUpgrades;
            //EventManager.InteractableEvents.GateInteract -= HandleWithGateInteract;
        }
        
        /*private void HandleWithGateInteract(GateInteractableData data)
        {
            // Initialize with default value
            float currentValue = 0.0f;

            // Get current value based on GateType
            switch (data.GateType)
            {
                case GateType.FireRate:
                    currentValue = _weaponDataSO.GetFireRate(playerUpgradeData.GetUpgradeLevel(_bulletDataSO.upgradeType));
                    break;
                case GateType.Damage:
                    currentValue = _bulletDataSO.GetDamage(playerUpgradeData.GetUpgradeLevel(_bulletDataSO.upgradeType));
                    float modifiedDamageValue = currentValue.ModifyValue(data.Amount, data.mathType);
                    _projectileStructData.damage = modifiedDamageValue;
                    
                    break;
                case GateType.Range:
                    currentValue = _bulletDataSO.GetRange(playerUpgradeData.GetUpgradeLevel(_bulletDataSO.upgradeType));
                    break;
                case GateType.Speed:
                    currentValue = _bulletDataSO.GetSpeed(playerUpgradeData.GetUpgradeLevel(_bulletDataSO.upgradeType));
                    break;
            }

            // Use the extension method to modify the value
        }*/

        private void OnUpgradeButtonClicked(UpgradeType upgradeType, UpgradeButtonStruct upgradeButtonStruct)
        {   
            
            if (upgradeButtonStruct.isResettable)
            {
                _levelTemporaryUpgrades.Add(upgradeType);
            }
            
            
            if (upgradeButtonStruct.fromRewardedAd)
            {
                PerformUpgrade(upgradeType,upgradeButtonStruct.increaseCount);
                EventManager.UpgradeSystem.CharacterUpgraded?.Invoke(upgradeType);
                return;
            }

            var upgradeCost = GetUpgradeCost(upgradeType);
            var collectableType =GetCollectableType(upgradeType);
            EventManager.CurrencySystem.TryToBuy?.Invoke(upgradeCost,collectableType, isBuySuccessful =>
            {
                if (isBuySuccessful)
                {
                    PerformUpgrade(upgradeType,upgradeButtonStruct.increaseCount);
                    EventManager.UpgradeSystem.CharacterUpgraded?.Invoke(upgradeType);
                }
            });
        }
        
        
        private CollectableType GetCollectableType(UpgradeType upgradeType)
        {
            foreach (var upgradable in upgradableSOs)
            {
                if (upgradable.UpgradeType == upgradeType)
                {
                    return upgradable.GetCollectableType(playerUpgradeData.GetUpgradeLevel(upgradeType));
                }
            }
            return CollectableType.Coin;
        }
        
        private int GetUpgradeCost(UpgradeType upgradeType)
        {
            foreach (var upgradable in upgradableSOs)
            {
                if (upgradable.UpgradeType == upgradeType)
                {
                    return upgradable.GetRequiredCurrencyForNextLevel(playerUpgradeData.GetUpgradeLevel(upgradeType));
                }
            }
            return 0;
        }

        private void PerformUpgrade(UpgradeType upgradeType,int increaseCount)
        {
            playerUpgradeData.IncreaseUpgradeLevel(upgradeType,increaseCount);
        }

        private void ResetUpgrades()
        {
            foreach (var temporaryUpgradeType in _levelTemporaryUpgrades)
            {
                var upgradable = upgradableSOs.Find(x => x.UpgradeType == temporaryUpgradeType);
                //playerUpgradeData.ResetToCacheValue();
            }
            _levelTemporaryUpgrades.Clear();
        }
        
        private void CacheUpgradesInitialValuesOnLevelStart()
        {
            foreach (var upgradableSO in upgradableSOs)
            {
                //playerUpgradeData.CacheLevel();
            }
        }
        
        
    }
    [Serializable]
    public enum UpgradeType
    {
        Speed,
        Health,
        Damage,
        BulletData,
        WeaponInterval
    }
}
