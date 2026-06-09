using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class SpawnObjectOnTakeDamage : ObjectSpawnProvider, IDamageableAction
    {
        #region Public Methods

        public void Initialize(DamageableObject damageableObject)
        {
            
        }
        
        public void TakeDamage(float damage)
        {
            CreateCurvyMovement(DamageEffectObject(this.transform.position));
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
