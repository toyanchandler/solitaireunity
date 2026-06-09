using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class SpawnObjectOnDeath : ObjectSpawnProvider, IDamageableAction
    {
        public void Initialize(DamageableObject damageableObject)
        {
            
        }

        public void TakeDamage(float damage)
        {
            
        }

        public void HealthChanged(float currentHealth)
        {
            
        }

        public void Death()
        {
            CreateCurvyMovement(DamageEffectObject(this.transform.position));
        }
    }
}