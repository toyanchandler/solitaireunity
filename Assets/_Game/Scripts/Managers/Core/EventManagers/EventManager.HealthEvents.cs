using _Game.Scripts.Template.GlobalProviders.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class HealthEvents
        {
            public static UnityAction<DamageableData> DamageableDeath;
        }
    }
}
