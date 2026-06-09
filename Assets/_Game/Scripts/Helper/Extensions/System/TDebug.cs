using UnityEngine;

namespace _Game.Scripts.Helper.Extensions.System
{
    public static class TDebug
    {
        #region Private Members

        private static bool TDebugMode;

        #endregion

        #region Static Constructor

        // This static constructor will run once on the first use of the TDebug class
        // and will correctly initialize the TDebugMode according to the build settings.
        static TDebug()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            TDebugMode = true;
#else
            TDebugMode = false;
#endif
        }

        #endregion

        #region Public Methods

        // Since TDebugMode is now non-readonly, you can modify it outside of the static constructor.
        public static void SetDebugMode(bool state)
        {
            TDebugMode = state;
        }

        public static void Log<T>(T message)
        {
            if (TDebugMode)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning<T>(T message)
        {
            if (TDebugMode)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError<T>(T message)
        {
            if (TDebugMode)
            {
                Debug.LogError(message);
            }
        }

        public static void LogAssertion<T>(T message)
        {
            if (TDebugMode)
            {
                Debug.LogAssertion(message);
            }
        }

        public static void LogErrorFormat<T>(T message, params object[] args)
        {
            if (TDebugMode)
            {
                Debug.LogErrorFormat(message.ToString(), args);
            }
        }

        public static void LogGreen<T>(T message)
        {
            if (TDebugMode)
            {
                Debug.Log($"<color=green>{message}</color>");
            }
        }

        #endregion
    }
}