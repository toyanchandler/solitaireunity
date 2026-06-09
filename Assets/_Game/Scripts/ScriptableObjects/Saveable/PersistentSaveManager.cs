using System.Reflection;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.RunTime;
using Handler.Extensions;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Saveable
{
    [ES3Serializable]
    public abstract class PersistentSaveManager<T> : SerializedScriptableObject, ISaveableProvider where T : class, IResettable
    {
        private T @object; // Object to store save data

        // Property to return save name based on instance ID
        private string SaveName => GetInstanceID().ToString();

        [SerializeField] private bool _overrideSave;

        // Load save data from storage
        public virtual void LoadData()
        {
            if (ES3.KeyExists(SaveName) && !_overrideSave)
            {
                @object = ES3.Load<T>(SaveName);
                CastAndApplyFields();
                EventManager.SaveEvents.DataLoaded?.Invoke();
            }
        }

        // Save data to storage
        public virtual void SaveData()
        {
            ES3.Save(SaveName, this as T);
            TDebug.Log("Saved " + SaveName);
            EventManager.SaveEvents.DataSaved?.Invoke();
        }

        public void ResetData()
        {
            ResetToInitialState();
        }

        private void CastAndApplyFields()
        {
            if (@object == null)
            {
                TDebug.LogWarning("The object is null. Aborting field setting.");
                return;
            }

            // Using Reflection to set fields
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                try
                {
                    if (field.Name != "name")
                    {
                        object value = field.GetValue(@object);
                        field.SetValue(this, value);
                    }
                }
                catch
                {
                    TDebug.LogWarning("Error loading field: " + field.Name);
                }
            }
        }
#if UNITY_EDITOR
        private string _initialJson = string.Empty;
#endif

        public void SaveInitialState()
        {
#if UNITY_EDITOR
            _initialJson = EditorJsonUtility.ToJson(this);
         //   TDebug.Log("Saved "+ _initialJson);
#endif
        }

        public void ResetToInitialState()
        {
#if UNITY_EDITOR
          //  TDebug.Log("RESETTED");
            EditorJsonUtility.FromJsonOverwrite(_initialJson, this);
#endif
        }
    }

    public interface ISaveableProvider
    {
        void LoadData();

        void SaveData();

        void ResetData();
    }
}
