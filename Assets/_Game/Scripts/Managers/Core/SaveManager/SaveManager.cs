using System;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class SaveManager : MonoBehaviour
    {
        [SerializeField] private InterfaceSerialization<ISaveableProvider> _playerSaveable;

        [SerializeField]
        private InterfaceSerialization<ISaveableProvider>[] _persistentSaveables =
            Array.Empty<InterfaceSerialization<ISaveableProvider>>();

        [SerializeField]
        private InterfaceSerialization<ISaveableProvider>[] _runtimeSaveables =
            Array.Empty<InterfaceSerialization<ISaveableProvider>>();

        private bool _isDirty;

        private void Awake()
        {
            ValidateReferences();
            LoadAll();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelSuccess += LevelEnd;
            EventManager.CurrencySystem.CollectableSpent += OnCollectableSpend;
        }


        private void OnDisable()
        {
            EventManager.InGameEvents.LevelSuccess -= LevelEnd;
            EventManager.CurrencySystem.CollectableSpent -= OnCollectableSpend;
        }

        private void OnCollectableSpend(CollectableType collectableType)
        {
            _isDirty = true;
            SaveAll();
        }   

        private void ValidateReferences()
        {
            if (_playerSaveable?.I == null)
            {
                TDebug.LogWarning($"{nameof(SaveManager)} requires a player saveable reference.");
            }

            ValidateGroupReferences(_persistentSaveables, "persistent");
            ValidateGroupReferences(_runtimeSaveables, "runtime");
        }

        private void ValidateGroupReferences(InterfaceSerialization<ISaveableProvider>[] saveables, string groupName)
        {
            if (saveables == null)
            {
                TDebug.LogWarning($"{nameof(SaveManager)} {groupName} saveable group is not assigned.");
                return;
            }

            for (int i = 0; i < saveables.Length; i++)
            {
                if (saveables[i]?.I == null)
                {
                    TDebug.LogWarning($"{nameof(SaveManager)} {groupName} saveable at index {i} is not assigned.");
                }
            }
        }

        private void LevelEnd()
        {
            SavePlayerAtLevelEnd();
        }

        private void OnStartAmountUpgrade()
        {
            _isDirty = true;
            SaveAll();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveIfDirty();
            }
        }

        private void OnApplicationQuit()
        {
            SaveIfDirty();
        }

        private void SavePlayerAtLevelEnd()
        {
            if (_playerSaveable?.I != null)
            {
                _playerSaveable.I.SaveData();
                _isDirty = false;
                return;
            }

            TDebug.LogWarning("SaveManager player saveable is not assigned. Saving persistent group for level end.");
            SavePersistent();
        }

        [Button]
        private void SaveAll()
        {
            SavePersistent();
            SaveRuntime();
            _isDirty = false;

        }

        [Button]
        private void LoadAll()
        {
            LoadPersistent();
            LoadRuntime();

        }

        [Button]
        private void SavePersistent()
        {
            SaveGroup(_persistentSaveables, "persistent");
        }

        [Button]
        private void LoadPersistent()
        {
            LoadGroup(_persistentSaveables, "persistent");
        }

        [Button]
        private void SaveRuntime()
        {
            SaveGroup(_runtimeSaveables, "runtime");
        }

        [Button]
        private void LoadRuntime()
        {
            LoadGroup(_runtimeSaveables, "runtime");
        }

        private void SaveIfDirty()
        {
            if (_isDirty)
            {
                SaveAll();
            }
        }

        private void SaveGroup(InterfaceSerialization<ISaveableProvider>[] saveables, string groupName)
        {
            if (saveables == null)
            {
                TDebug.LogWarning($"SaveManager {groupName} saveable group is not assigned.");
                return;
            }

            foreach (InterfaceSerialization<ISaveableProvider> saveable in saveables)
            {
                if (saveable?.I == null)
                {
                    TDebug.LogWarning($"SaveManager found a null {groupName} saveable while saving.");
                    continue;
                }

                saveable.I.SaveData();
            }
        }

        private void LoadGroup(InterfaceSerialization<ISaveableProvider>[] saveables, string groupName)
        {
            if (saveables == null)
            {
                TDebug.LogWarning($"SaveManager {groupName} saveable group is not assigned.");
                return;
            }

            foreach (InterfaceSerialization<ISaveableProvider> saveable in saveables)
            {
                if (saveable?.I == null)
                {
                    TDebug.LogWarning($"SaveManager found a null {groupName} saveable while loading.");
                    continue;
                }

                saveable.I.LoadData();
            }
        }

#if UNITY_EDITOR
        [Button][GUIColor( 0.8f, 0.3f, 0.3f, 1f)]
        void ClearPersistentDataPath()
        {
            ES3.DeleteFile();
        }
#endif
    }
}
