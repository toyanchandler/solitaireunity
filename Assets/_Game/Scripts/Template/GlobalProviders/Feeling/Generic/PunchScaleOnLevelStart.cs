using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class PunchScaleOnLevelStart : PunchScaleProvider
    {
        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += StartPunchScale;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= StartPunchScale;
        }

        #endregion

        #region Private Methods

        private void StartPunchScale(GameObject go) => PunchScale();
        
        #endregion
    }
}