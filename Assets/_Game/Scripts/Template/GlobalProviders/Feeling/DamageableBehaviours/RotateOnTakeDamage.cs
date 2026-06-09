using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class RotateOnTakeDamage : ObjectRotateProvider, IDamageableAction
    {
        #region Private Variables

        private bool isCoroutineStarted = false;

        #endregion
        
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
            if (isCoroutineStarted) return;
            StartRotateCoroutine();
            isCoroutineStarted = true;
        }

        #endregion
    }
}