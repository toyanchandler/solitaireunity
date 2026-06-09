using System.Collections.Generic;

namespace _Game.Scripts.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName);
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters);
    }
}
