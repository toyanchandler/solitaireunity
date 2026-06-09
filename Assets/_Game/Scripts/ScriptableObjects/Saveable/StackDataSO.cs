using System.Collections.Generic;
using _Game.Scripts.ScriptableObjects.RunTime;
using _Game.Scripts.Template.GlobalProviders.Interactable.Stacking;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Saveable
{
    [CreateAssetMenu(fileName = "StackData", menuName = "ThisGame/StackData", order = 0)]
    public class StackDataSO : PersistentSaveManager<StackDataSO>, IResettable
    {
        [OdinSerialize] private Dictionary<StackableData.StackableType, int> stackableCount =
            new Dictionary<StackableData.StackableType, int>();

        public void RegisterStackableData(StackableData.StackableType type, int amount)
        {
            if (stackableCount.ContainsKey(type))
            {
                stackableCount[type] += amount;
            }
            else
            {
                stackableCount[type] = 1;
            }
        }

        public void UnregisterStackableData(StackableData.StackableType type, int amount)
        {
            if (!stackableCount.ContainsKey(type))
            {
                return;
            }

            stackableCount[type] -= amount;
            if (stackableCount[type] <= 0)
            {
                stackableCount.Remove(type);
            }
        }
    }
}
