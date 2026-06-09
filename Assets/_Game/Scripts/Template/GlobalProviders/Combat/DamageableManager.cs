using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Health;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public class DamageableManager : MonoBehaviour
    {
        [SerializeField]
        private DeathValueSO deathValueSO;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EventManager.HealthEvents.DamageableDeath += HandleDeathEvent;
        }

        private void UnsubscribeEvents()
        {
            EventManager.HealthEvents.DamageableDeath -= HandleDeathEvent;
        }

        private void HandleDeathEvent(DamageableData damageableData)
        {
            deathValueSO.AddValueByOne(damageableData.Type);
        }
    }
}
