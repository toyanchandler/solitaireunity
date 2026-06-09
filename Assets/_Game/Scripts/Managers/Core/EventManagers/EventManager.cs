using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class InGameEvents
        {
            public static UnityAction GameStarted;
            public static UnityAction LoadLevel;
            public static UnityAction BeforeLevelLoaded;
            public static UnityAction<GameObject> LevelLoaded;
            public static UnityAction LevelStart;
            public static UnityAction LevelSuccess;
            public static UnityAction EndMetaStart;
            public static UnityAction LevelRestart;
            public static UnityAction LevelFail;
        }
        
        public static class SaveEvents
        {
            public static UnityAction DataSaved;
            public static UnityAction DataLoaded;
        }
        
        public static class AudioEvents
        {
            public static UnityAction<int, AudioClip> AudioAdded;
            public static UnityAction AudioStop;
            public static UnityAction<int> AudioPlay;
            public static UnityAction<float> VolumeChange;
            public static UnityAction<int, float> AudioChanged;
            public static UnityAction<bool> AudioLoopToggleChanged;
            public static UnityAction<bool> AudioEnabled;
        }
        
        public static class AdEvents
        {
            public static UnityAction<Action> RewardedShow;
            public static UnityAction InterstitialReward;
        }
    }
}