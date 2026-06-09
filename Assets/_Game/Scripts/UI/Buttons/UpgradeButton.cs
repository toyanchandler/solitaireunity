using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Predefined;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Buttons
{
    public class UpgradeButton : ButtonBase
    {
        [SerializeField] private UpgradableSO _upgradableSO;
        [SerializeField] private UpgradeButtonStruct _upgradeButtonStruct;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI upgradeLevelText;
        [SerializeField] private Image costIconImage;
        [SerializeField] private CollectableValuesSO collectableValuesSO;
        [SerializeField] private IconProviderSO iconProvider;
        [SerializeField] private Image rewardedAdImage;
        [SerializeField] private PlayerUpgradeData _playerUpgradeData;
        [SerializeField] private TextMeshProUGUI _upgradeTypeText;
        

     
        private void OnEnable()
        {
            SubscribeEvents();
            ResetButtonProperties();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            EventManager.CurrencySystem.CollectableSpent += OnCollectableSpent;
            EventManager.UpgradeSystem.CharacterUpgraded += OnCharacterUpgraded;

         
        }
        
        
        private void UnsubscribeEvents()
        {
            EventManager.CurrencySystem.CollectableSpent -= OnCollectableSpent;
            EventManager.UpgradeSystem.CharacterUpgraded -= OnCharacterUpgraded;
        }
        
        private void OnCharacterUpgraded(UpgradeType upgradeType)
        {
            if (upgradeType == _upgradableSO.UpgradeType)
            {
                ResetButtonProperties();
            }
        }
        
        private void OnCollectableSpent(CollectableType collectableType)
        {
            if (collectableType == _upgradableSO.CollectableType)
            {
                ResetButtonProperties();
            }
        }
        

        private void SetIcon()
        {
            iconImage.sprite = iconProvider.GetUpgradeIcon(_upgradableSO.UpgradeType);
        }

        private void SetCostAndCostImage()
        {
            int cost = _upgradableSO.GetRequiredCurrencyForNextLevel(_playerUpgradeData.GetUpgradeLevel(_upgradableSO.UpgradeType));
            costText.text = cost.ToString();
            
            Sprite costSprite = iconProvider.GetCollectableIcon(_upgradableSO.GetCollectableType(_playerUpgradeData.GetUpgradeLevel(_upgradableSO.UpgradeType)));
            costIconImage.sprite = costSprite;
        }

        private void SetUpgradeTypeText()
        {
            _upgradeTypeText.text = _upgradableSO.UpgradeType.ToString();
        }
        private void CheckAvailability()
        {
            if (_upgradableSO.GetRequiredCurrencyForNextLevel(_playerUpgradeData.GetUpgradeLevel(_upgradableSO.UpgradeType))<=collectableValuesSO.GetValue(_upgradableSO.CollectableType))
            {
                rewardedAdImage.gameObject.SetActive(false);
                TargetButton.onClick.RemoveAllListeners();
                TargetButton.onClick.AddListener(OnClicked);
            }
            else
            {
                SetRewardedAddState();
            }
        }
        
        private void SetRewardedAddState()
        {
           rewardedAdImage.gameObject.SetActive(true);
           TargetButton.onClick.RemoveAllListeners();
           TargetButton.onClick.AddListener(OnRewardedAdClicked);
        }
        
        private void OnRewardedAdClicked()
        {
            EventManager.AdEvents.RewardedShow?.Invoke(OnRewardedAdCompleted);
            
        }
        
        private void OnRewardedAdCompleted()
        {
            _upgradeButtonStruct.fromRewardedAd = true;
            EventManager.UpgradeSystem.UpgradeButtonClicked?.Invoke(_upgradableSO.UpgradeType,_upgradeButtonStruct);
            _upgradeButtonStruct.fromRewardedAd = false;
            rewardedAdImage.gameObject.SetActive(false);
            ResetButtonProperties();

        }
        
        [Button]
        public void ResetButtonProperties()
        {
            SetIcon();
            SetCostAndCostImage();
            CheckAvailability();
            SetUpgradeTypeText();
            SetUpgradeLevelText();
            _upgradeButtonStruct.increaseCount=1;
        }
        
        private void SetUpgradeLevelText()
        {
            upgradeLevelText.text = "LVL "+ _playerUpgradeData.GetUpgradeLevel(_upgradableSO.UpgradeType).ToString();
        }

        protected override void OnClicked()
        {
            EventManager.UpgradeSystem.UpgradeButtonClicked?.Invoke(_upgradableSO.UpgradeType,_upgradeButtonStruct);
            ResetButtonProperties();
        }
    }

    [Serializable]
    public struct UpgradeButtonStruct
    {
        public bool isResettable;
        public bool fromRewardedAd;
        public int increaseCount;
    }
}
