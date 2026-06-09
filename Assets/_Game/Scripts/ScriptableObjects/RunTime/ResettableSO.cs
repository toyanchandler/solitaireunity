#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.RunTime
{
    public abstract class ResettableScriptableObject : ScriptableObject, IResettable
    {
#if UNITY_EDITOR
        private string _initialJson = string.Empty;
    
#endif

        public void SaveInitialState()
        {
#if UNITY_EDITOR
            _initialJson = EditorJsonUtility.ToJson(this);
#endif
        }

        public void ResetToInitialState()
        {
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(_initialJson, this);
#endif
        }
    }
}
