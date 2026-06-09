using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Saveable
{
    public class UpgradableSO : SerializedScriptableObject
    {
        [SerializeField] private UpgradeLevelData[] upgradeLevelData = System.Array.Empty<UpgradeLevelData>();
        
        [SerializeField] private UpgradeType upgradeType;

        [SerializeField] private CollectableType collectableType;
        
        [SerializeField] private bool isFormulaWillBeUsed;
        [SerializeField] private float formulaValueA;
        [SerializeField] private float formulaValueB;
        [SerializeField] private float formulaValueC;
        [SerializeField] private float formulaCostA;
        [SerializeField] private float formulaCostB;
        [SerializeField] private float formulaCostC;

        public UpgradeType UpgradeType => upgradeType;
        public CollectableType CollectableType => collectableType;
        
        [Button]
        public virtual void SetAllLevelsCollecabletype()
        {
            for (int i = 0; i < upgradeLevelData.Length; i++)
            {
                upgradeLevelData[i].SetCollectableType(collectableType);
            }
        }
        public virtual int GetRequiredCurrencyForNextLevel(int currentLevel)
        {
            if (isFormulaWillBeUsed)
            {
                return Mathf.RoundToInt(formulaCostA * Mathf.Pow(currentLevel,2)+ formulaCostB*currentLevel+ formulaCostC);
            }
            else
            {
                return GetLevelData(currentLevel).RequiredCurrencyForNextLevel;
            }
        }
    
        
        public virtual CollectableType GetCollectableType(int currentLevel)
        {
            if (isFormulaWillBeUsed)
                return collectableType;
            
            return GetLevelData(currentLevel).CollectableType;
        }
        
        public virtual float GetValue(int currentLevel)
        {
            if (isFormulaWillBeUsed)
            {
                //ax^2+bx+c
                return formulaValueA * Mathf.Pow(currentLevel,2)+formulaValueB*currentLevel+formulaValueC;
            }
            else
            {
                return GetLevelData(currentLevel).Value;
            }
        }
        
        public virtual int GetMaxLevel()
        {
            return upgradeLevelData.Length - 1;
        }

        private UpgradeLevelData GetLevelData(int currentLevel)
        {
            if (upgradeLevelData == null || upgradeLevelData.Length == 0)
            {
                throw new System.InvalidOperationException($"{name} has no upgrade level data.");
            }

            var clampedLevel = Mathf.Clamp(currentLevel, 0, upgradeLevelData.Length - 1);
            return upgradeLevelData[clampedLevel];
        }
        
    }
    
    
    [System.Serializable]
    public class UpgradeLevelData
    {
        [SerializeField] private int requiredCurrencyForNextLevel;

        [SerializeField] private float value;

        [SerializeField] private CollectableType collectableType;

        public int RequiredCurrencyForNextLevel => requiredCurrencyForNextLevel;
        public float Value => value;
        public CollectableType CollectableType => collectableType;

        public void SetCollectableType(CollectableType type)
        {
            collectableType = type;
        }
    }
}
