using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Combat;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Health
{
    [RequireComponent(typeof(DamageableObject))]
    public abstract class HealthProvider : MonoBehaviour, IHealth, IDamageableAction
    {
        #region Public Variables

        [SerializeField] protected DamageableData _damageableData;
        public float CurrentHealth => _damageableData.CurrentHealth;

        #endregion

        #region Private Variables

        private DamageableObject _damageableObject;

        #endregion

        #region Virtual Methods

        protected virtual void OnDeath() => _damageableObject.CanReceiveDamage = false;

        public virtual void TakeDamage(float damage)
        {
            _damageableData.ApplyDamage(damage);
            OnHealthChanged(_damageableData.CurrentHealth);

            if (_damageableData.CurrentHealth <= 0)
            {
                _damageableObject.Death();
            }
        }

        #endregion

        #region Private Methods

        private void OnHealthChanged(float newHealth)
        {
            _damageableObject.DamageableHealthChanged(newHealth);
        }

        #endregion

        #region Public Methods
        
        public void Death() => OnDeath();

        public void Initialize(DamageableObject damageableObject)
        {
            this._damageableObject = damageableObject;
            
            _damageableData.ResetHealth();
            
            damageableObject.SetDamageableData(_damageableData);
        }

        public virtual void Heal(float amount)
        {
            _damageableData.Heal(amount);
            OnHealthChanged(_damageableData.CurrentHealth);
        }
        
        public void HealthChanged(float currentHealth){}

        #endregion
    }
}
