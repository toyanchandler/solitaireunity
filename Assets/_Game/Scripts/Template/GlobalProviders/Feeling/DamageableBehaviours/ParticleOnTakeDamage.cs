using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class ParticleOnTakeDamage : ParticleProvider, IDamageableAction
    {
        #region Public Methods

        public void Initialize(DamageableObject damageableObject)
        {
        }

        public void TakeDamage(float damage)
        {
            PlayParticle();
        }

        public void HealthChanged(float currentHealth)
        {
        }

        public void Death()
        {
            
        }

        #endregion
    }
}