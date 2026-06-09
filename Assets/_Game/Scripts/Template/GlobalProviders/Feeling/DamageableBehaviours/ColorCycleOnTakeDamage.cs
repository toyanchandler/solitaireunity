using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Feeling.Generic;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class ColorCycleOnTakeDamage : ColorCycleProvider, IDamageableAction
    {
        #region Public Methods

        public void Initialize(DamageableObject damageableObject)
        {
        }

        public void TakeDamage(float damage)
        {
            StartCoroutine();
        }

        public void HealthChanged(float currentHealth)
        {
        }

        public void Death()
        {
        }

        #endregion

        #region Private Methods

        private void StartCoroutine()
        {
            if(gameObject.activeSelf)
                StartColorCycle();
        }

        #endregion
        
    }
}