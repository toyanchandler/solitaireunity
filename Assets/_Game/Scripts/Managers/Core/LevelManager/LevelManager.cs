using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.ScriptableObjects.Predefined;
using _Game.Scripts.ScriptableObjects.Saveable;
using Handler.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class LevelManager : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private LevelList_SO _levelListSO;
        [SerializeField] private PlayerSaveableData _playerSaveableData;
        [SerializeField] private Transform _levelHolder;
        [ReadOnly] [SerializeField] private GameObject _levelGO;
        [SerializeField] private bool _spawnFromLevelList;

        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.GameStarted += LoadLevel;
            EventManager.InGameEvents.LoadLevel += LoadLevel;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.GameStarted -= LoadLevel;
            EventManager.InGameEvents.LoadLevel -= LoadLevel;
        }

        #endregion
        
        #region Private Methods

        private void ValidateReferences()
        {
            if (_levelHolder == null)
            {
                TDebug.LogWarning($"{nameof(LevelManager)} requires {nameof(_levelHolder)}.");
            }

            if (_spawnFromLevelList && _levelListSO == null)
            {
                TDebug.LogWarning($"{nameof(LevelManager)} is configured to spawn from list but {nameof(_levelListSO)} is missing.");
            }

            if (_spawnFromLevelList && _playerSaveableData == null)
            {
                TDebug.LogWarning($"{nameof(LevelManager)} is configured to spawn from list but {nameof(_playerSaveableData)} is missing.");
            }
        }

        private void LoadLevel()
        {
            EventManager.InGameEvents.BeforeLevelLoaded?.Invoke();
            DestroyExistingLevel();
            LoadActiveLevel();
            NotifyLevelLoaded();
        }

        private void DestroyExistingLevel()
        {
            if (_levelGO != null && _levelGO.transform.parent == _levelHolder)
            {
                Destroy(_levelGO);
                _levelGO = null;
            }
        }
        
        private GameObject TryGetLevelFromHolder()
        {
            if (_levelHolder == null)
            {
                TDebug.LogWarning($"{nameof(LevelManager)} cannot read level holder because it is missing.");
                return null;
            }

            GameObject level = _levelHolder.childCount > 0 ? _levelHolder.GetChild(0).gameObject : null;
            
            _levelGO = level;
            
            return level;
        }

        private void LoadActiveLevel()
        {
            if (_spawnFromLevelList)
            {
                InstantiateLevelFromList();
                return;
            }

            _levelGO = TryGetLevelFromHolder();
        }

        private void InstantiateLevelFromList()
        {
            GameObject levelPrefab = TryGetLevelFromSoWithIndexedLevel();
            if (levelPrefab == null)
            {
                TDebug.LogError($"{nameof(LevelManager)} could not resolve a level prefab from LevelList.");
                return;
            }

            if (_levelHolder == null)
            {
                TDebug.LogError($"{nameof(LevelManager)} cannot instantiate level because level holder is missing.");
                return;
            }

            _levelGO = Instantiate(levelPrefab, _levelHolder.position, Quaternion.identity, _levelHolder);
        }
        
        private GameObject TryGetLevelFromSoWithIndexedLevel()
        {
            if (_levelListSO == null || _playerSaveableData == null)
            {
                return null;
            }

            Level_SO levelData = _levelListSO.GetLevelWithIndex(_playerSaveableData.LevelIndex);
            GameObject level = levelData != null ? levelData.LevelPrefab : null;
            
            _levelGO = level;
            
            return level;
        }

        private GameObject TryGetLevel()
        {
            GameObject level = _spawnFromLevelList ? _levelGO : TryGetLevelFromHolder();
            
            _levelGO = level;
            
            return level;
        }
            
        
        private void NotifyLevelLoaded()
        {
            GameObject loadedLevel = _levelGO != null ? _levelGO : TryGetLevel();
            if (loadedLevel == null)
            {
                TDebug.LogError($"{nameof(LevelManager)} cannot notify LevelLoaded because no level is loaded.");
                return;
            }

            EventManager.InGameEvents.LevelLoaded?.Invoke(loadedLevel);
            
            TDebug.LogGreen($"{loadedLevel.name} is loaded");
        }

        #endregion
    }
}
