using System;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public class DamageableObject : MonoBehaviour, IDamageable
    {
        #region Inspector Variables

        [SerializeField] private MonoBehaviour[] actionComponents = Array.Empty<MonoBehaviour>();

        [ShowInInspector, ReadOnly] private IDamageableAction[] actions = Array.Empty<IDamageableAction>();
        
        [SerializeField] private bool canInteract = true;

        [SerializeField] private DamageableData damageableData;
        
        #endregion
        
        public bool CanReceiveDamage
        { 
            get => canInteract;
            set => canInteract = value;
        }

        public DamageableData DamageableData => damageableData;

        #region Unity Methods

        private void Awake()
        {
            actions = GlobalProviderGuard.BuildActionCache<IDamageableAction>(this, actionComponents);
            
            foreach (var action in actions)
            {
                action.Initialize(this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            actionComponents = GlobalProviderGuard.CollectChildActions<IDamageableAction>(this);
        }
#endif
        
        #endregion
        
        public void TakeDamage(float damage)
        {
            if (!CanReceiveDamage) return;

            if (actions == null || actions.Length == 0)
            {
                TDebug.LogError("No IDamageableAction found on " + gameObject.name);
                return;
            }
            
            foreach (var action in actions)
            {
                action.TakeDamage(damage);
            }
        }
        
        public void Death()
        {
            EventManager.HealthEvents.DamageableDeath?.Invoke(damageableData);

            CanReceiveDamage = false;
            foreach (var action in actions)
            {
                action.Death();
            }
        }

        public void SetDamageableData(DamageableData data)
        {
            damageableData = data;
        }
        
        public void DamageableHealthChanged(float currentHealth)
        {
            foreach (var action in actions)
            {
                action.HealthChanged(currentHealth);
            }
        }
    }
    
    [Serializable]
    public struct DamageableData
    {
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        [SerializeField] private float maxHealth;

        [SerializeField] private float currentHealth;
        
        public enum DamageableType
        {
            Player,
            Enemy,
            Environment
        }
        
        [GUIColor(1f, 0.3f, 0.3f, 1f)]
        [SerializeField] private DamageableType damageableType;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public DamageableType Type => damageableType;

        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }

        public void ApplyDamage(float damage)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damage);
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }
    }
}
