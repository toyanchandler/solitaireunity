using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace _Game.Scripts.Template.GlobalProviders.Health
{
    public sealed class Health : HealthProvider
    {
        #region Inherited Methods

        [Button]
        public override void TakeDamage(float damage) => base.TakeDamage(damage);

        [Button]
        public override void Heal(float amount) => base.Heal(amount);
        
        protected override void OnDeath()
        {
            base.OnDeath();
            Destroy(gameObject);
        }

        #endregion
    }
}