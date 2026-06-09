using _Game.Scripts.Template.GlobalProviders.Input;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class ClickableEvents
        {
            public static UnityAction<ClickData> ClickDown;
            public static UnityAction<ClickData> ClickHold;
            public static UnityAction<ClickData> ClickUp;
        }
    }
}
