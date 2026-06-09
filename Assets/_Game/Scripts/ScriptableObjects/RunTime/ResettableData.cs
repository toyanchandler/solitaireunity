using _Game.Scripts.Helper.Extensions.System;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.RunTime
{
    [CreateAssetMenu(fileName = "ResettableData", menuName = "ThisGame/ResettableData", order = 0)]
    public class ResettableData : ScriptableObject
    {
        public InterfaceSerialization<IResettable>[] resettableData;

        public void ResetAllData()
        {
            foreach (var resettable in resettableData)
            {
                resettable.I.ResetToInitialState();
            }
        }
    }
}