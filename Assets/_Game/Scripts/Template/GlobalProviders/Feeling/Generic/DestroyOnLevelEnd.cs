using System;
using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class DestroyOnLevelEnd : MonoBehaviour
    {
        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelSuccess += DestroyOnLevelEndMethod;
            EventManager.InGameEvents.LevelFail += DestroyOnLevelEndMethod;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelSuccess -= DestroyOnLevelEndMethod;
            EventManager.InGameEvents.LevelFail -= DestroyOnLevelEndMethod;
        
        }

        #endregion

        #region Private Methods

        private void DestroyOnLevelEndMethod()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}