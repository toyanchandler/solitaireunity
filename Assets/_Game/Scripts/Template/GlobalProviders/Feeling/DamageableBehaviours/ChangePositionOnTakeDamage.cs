using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class ChangePositionOnTakeDamage : TransformUpdateProvider, IDamageableAction
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
            StartChangePosition();
        }

        public void HealthChanged(float currentHealth)
        {
            
        }

        public void Death()
        {
            
        }

        #endregion

        #region Private Methods

        private void StartChangePosition()
        {
            if (isCoroutineStarted) return;
            ChangePosition();
            isCoroutineStarted = true;
        }

        #endregion
        
    }
}