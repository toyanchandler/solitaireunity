using System.Collections.Generic;

namespace _Game.Scripts.General.AnalyticsManager
{
    public sealed class CompositeAnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsService[] _services;

        public CompositeAnalyticsService(params IAnalyticsService[] services)
        {
            _services = services;
        }

        public void LogEvent(string eventName)
        {
            foreach (IAnalyticsService service in _services)
            {
                service?.LogEvent(eventName);
            }
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            foreach (IAnalyticsService service in _services)
            {
                service?.LogEvent(eventName, parameters);
            }
        }
    }
}
