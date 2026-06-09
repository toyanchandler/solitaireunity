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
            LoadAll();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelSuccess += SavePlayerAtLevelEnd;
            EventManager.CurrencySystem.CollectableSpent += HandleCollectableSpent;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelSuccess -= SavePlayerAtLevelEnd;
            EventManager.CurrencySystem.CollectableSpent -= HandleCollectableSpent;
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

        private void HandleCollectableSpent(CollectableType collectableType)
        {
            _isDirty = true;
            SaveAll();
        }

        private void SavePlayerAtLevelEnd()
        {
            if (_playerSaveable?.I == null)
            {
                TDebug.LogWarning("Player saveable is not assigned.");
                return;
            }

            _playerSaveable.I.SaveData();
            _isDirty = false;
        }

        [Button]
        private void SaveAll()
        {
            SaveGroup(_persistentSaveables);
            SaveGroup(_runtimeSaveables);
            _isDirty = false;
        }

        [Button]
        private void LoadAll()
        {
            LoadGroup(_persistentSaveables);
            LoadGroup(_runtimeSaveables);
        }

        private void SaveIfDirty()
        {
            if (_isDirty)
            {
                SaveAll();
            }
        }

        private void SaveGroup(InterfaceSerialization<ISaveableProvider>[] saveables)
        {
            foreach (InterfaceSerialization<ISaveableProvider> saveable in saveables)
            {
                if (saveable?.I == null)
                {
                    TDebug.LogWarning("SaveManager found a null saveable while saving.");
                    continue;
                }

                saveable.I.SaveData();
            }
        }

        private void LoadGroup(InterfaceSerialization<ISaveableProvider>[] saveables)
        {
            foreach (InterfaceSerialization<ISaveableProvider> saveable in saveables)
            {
                if (saveable?.I == null)
                {
                    TDebug.LogWarning("SaveManager found a null saveable while loading.");
                    continue;
                }

                saveable.I.LoadData();
            }
        }
    }
}
