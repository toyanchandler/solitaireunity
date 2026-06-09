using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class RotateOnLevelStart : ObjectRotateProvider
    {
        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += StartRotateCoroutine;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= StartRotateCoroutine;
        }

        #endregion

        #region Private Methods

        private void StartRotateCoroutine(GameObject go)
        {
            StartRotateCoroutine();
        }

        #endregion
    }
}