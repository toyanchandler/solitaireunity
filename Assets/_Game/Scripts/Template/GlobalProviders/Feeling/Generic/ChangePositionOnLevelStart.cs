using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class ChangePositionOnLevelStart : TransformUpdateProvider
    {
        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += StartChangePosition;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= StartChangePosition;
        }

        #endregion

        #region Private Methods

        private void StartChangePosition(GameObject go) => ChangePosition();

        #endregion
    }
}