using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class TimerEvents
        {
            public static UnityAction LevelTimerBegin;
            public static UnityAction<float> LevelTimerEnd;
        }
    }
}
