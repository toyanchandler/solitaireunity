using System.Collections.Generic;
using System.Text;
using _Game.Scripts.Helper.Extensions.System;
using Handler.Extensions;

namespace _Game.Scripts.General.AnalyticsManager
{
    public sealed class DebugAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName)
        {
            TDebug.Log($"Analytics event: {eventName}");
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                LogEvent(eventName);
                return;
            }

            TDebug.Log($"Analytics event: {eventName} {FormatParameters(parameters)}");
        }

        private static string FormatParameters(IReadOnlyDictionary<string, object> parameters)
        {
            var builder = new StringBuilder();
            builder.Append('{');

            bool appendSeparator = false;
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                if (appendSeparator)
                {
                    builder.Append(", ");
                }

                builder.Append(parameter.Key);
                builder.Append(": ");
                builder.Append(parameter.Value);
                appendSeparator = true;
            }

            builder.Append('}');
            return builder.ToString();
        }
    }
}
