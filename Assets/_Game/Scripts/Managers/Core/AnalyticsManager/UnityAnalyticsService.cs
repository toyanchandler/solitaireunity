using System.Collections.Generic;

namespace _Game.Scripts.General.AnalyticsManager
{
    public sealed class UnityAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName)
        {
            LogEvent(eventName, null);
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
        }
    }

    public interface IAnalyticsService
    {
        void LogEvent(string eventName);
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters);
    }
}
