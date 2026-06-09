using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using DG.Tweening.Core;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class ColorCycleOnLevelStart : ColorCycleProvider
    {
        #region Unity Methods
        
        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += StartColorCycleGO;
            EventManager.InGameEvents.LevelSuccess += StopColorCycle;
            EventManager.InGameEvents.LevelFail += StopColorCycle;
        }
        
        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= StartColorCycleGO;
            EventManager.InGameEvents.LevelSuccess -= StopColorCycle;
            EventManager.InGameEvents.LevelFail -= StopColorCycle;
        }
        
        private void StartColorCycleGO(GameObject go)
        {
            StartColorCycle();
        }
        
        #endregion
    }
}