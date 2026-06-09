using _Game.Scripts.Template.GlobalProviders.Combat;
using Fluxy;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class ShootableEvents
        {
            public static UnityAction<WeaponStructData> Shoot;
            public static UnityAction<FluxyTarget> FluidOnShoot;
        }
    }
}