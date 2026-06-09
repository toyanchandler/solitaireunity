using _Game.Scripts.General;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders;
using _Game.Scripts.Template.GlobalProviders.Combat;
using Handler.Extensions;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.EndMeta
{
    public class EndMetaObstacle : MonoBehaviour, IInteractableAction, IDamageableAction
    {
        #region Private Fields

        [SerializeField] private TextMeshPro _healthText;
        
        #endregion

        #region Public Methods

        public void OnInteract()
        {
            EventManager.InGameEvents.LevelSuccess?.Invoke();
        }

        public void Initialize(DamageableObject damageableObject)
        {
            GlobalProviderGuard.Require(_healthText, this, nameof(_healthText));
            _healthText.text = $"{damageableObject.DamageableData.CurrentHealth.ToInt()}";
        }

        public void TakeDamage(float damage)
        {
            
        }

        public void Death()
        {
            Destroy(gameObject);
        }
        
        public void HealthChanged(float currentHealth)
        {
            _healthText.text = $"{currentHealth.ToInt()}";
        }

        #endregion
    }
}
