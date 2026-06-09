using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class PlayParticleOnLevelStart : ParticleProvider
    {
        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += StartParticle;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= StartParticle;
        }

        #endregion

        #region Private Methods

        private void StartParticle(GameObject go) => PlayParticle();
        
        
        #endregion
    }
}