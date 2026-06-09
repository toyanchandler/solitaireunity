using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class ScaleOnLevelStart : ObjectScaleProvider
    {
        #region Unity Methods

        private void OnEnable() => EventManager.InGameEvents.LevelLoaded += StartScaleOnLevelLoaded;

        private void OnDisable() => EventManager.InGameEvents.LevelLoaded -= StartScaleOnLevelLoaded;

        #endregion

        #region Private Methods

        private void StartScaleOnLevelLoaded(GameObject go)
        {
            StartScaleCoroutine();
        }

        #endregion
    }
}