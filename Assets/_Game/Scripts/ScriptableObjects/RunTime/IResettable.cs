#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.RunTime
{

// Define an interface for objects that can be reset to their initial state
    public interface IResettable
    {
        void SaveInitialState();
        void ResetToInitialState();
    }

#if UNITY_EDITOR

// This class listens to changes in play mode to save and reset state for IResettable objects.
    [InitializeOnLoad]
    public class PlayModeStateListener
    {
        static PlayModeStateListener()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnPlayMode()
        {
            SaveInitialStates();
        }

        // Helper method to save initial states for all IResettable objects
        private static void SaveInitialStates()
        {
            var resettables = FindAllResettables();
            foreach (var resettable in resettables)
            {
                resettable.SaveInitialState();
            }
        }
        
        // Helper method to reset to initial states for all IResettable objects
        private static void ResetToInitialStates()
        {
            var resettables = FindAllResettables();
            foreach (var resettable in resettables)
            {
                resettable.ResetToInitialState();
            }
        }

        // Helper method to find all objects implementing IResettable interface
        private static IResettable[] FindAllResettables()
        {
            var allScriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            return allScriptableObjects.OfType<IResettable>().ToArray();
        }

        // This method gets called whenever the play mode changes
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    ResetToInitialStates();
                    break;
            }
        }
    }
#endif
}
