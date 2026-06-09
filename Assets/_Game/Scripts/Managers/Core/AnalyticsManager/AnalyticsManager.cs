using System.Collections.Generic;
using _Game.Scripts.General.AnalyticsManager;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.ScriptableObjects.Saveable;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class AnalyticsManager : MonoBehaviour
    {
        private const int UnknownLevelIndex = -1;

        [SerializeField] private PlayerSaveableData _playerSaveableData;

        private IAnalyticsService _analyticsService;

        private void Awake()
        {
            ValidateReferences();
            Initialize();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Initialize()
        {
            _analyticsService = new CompositeAnalyticsService(
                new UnityAnalyticsService(),
                new DebugAnalyticsService()
            );
        }

        private void ValidateReferences()
        {
            if (_playerSaveableData == null)
            {
                TDebug.LogWarning($"{nameof(AnalyticsManager)} requires {nameof(_playerSaveableData)} for level payloads.");
            }
        }

        private void Subscribe()
        {
            EventManager.InGameEvents.GameStarted += LogGameStart;
            EventManager.InGameEvents.LevelStart += LogLevelStart;
            EventManager.InGameEvents.LevelSuccess += LogLevelSuccess;
            EventManager.InGameEvents.LevelFail += LogLevelFail;
            EventManager.SaveEvents.DataLoaded += LogDataLoaded;
            EventManager.InGameEvents.LevelLoaded += LogLevelLoaded;
        }

        private void Unsubscribe()
        {
            EventManager.InGameEvents.GameStarted -= LogGameStart;
            EventManager.InGameEvents.LevelStart -= LogLevelStart;
            EventManager.InGameEvents.LevelSuccess -= LogLevelSuccess;
            EventManager.InGameEvents.LevelFail -= LogLevelFail;
            EventManager.SaveEvents.DataLoaded -= LogDataLoaded;
            EventManager.InGameEvents.LevelLoaded -= LogLevelLoaded;
        }

        #region Logs

        private void LogGameStart()
        {
            LogEvent("GameStart");
        }

        private void LogLevelStart()
        {
            LogEvent("LevelStart", CreateLevelPayload());
        }

        private void LogLevelSuccess()
        {
            LogEvent("LevelSuccess", CreateLevelPayload());
        }
        
        private void LogLevelFail()
        {
            LogEvent("LevelFail", CreateLevelPayload());
        }

        private void LogDataLoaded()
        {
            LogEvent("DataLoaded");
        }

        private void LogLevelLoaded(GameObject levelGameObject)
        {
            LogEvent("LevelLoaded", new Dictionary<string, object>
            {
                ["levelName"] = levelGameObject != null ? levelGameObject.name : "null"
            });
        }

        private Dictionary<string, object> CreateLevelPayload()
        {
            return new Dictionary<string, object>
            {
                ["level"] = _playerSaveableData != null ? _playerSaveableData.LevelIndex : UnknownLevelIndex
            };
        }

        private void LogEvent(string eventName)
        {
            if (_analyticsService == null)
            {
                TDebug.LogWarning("Analytics Service not initialized.");
                return;
            }

            _analyticsService.LogEvent(eventName);
        }

        private void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (_analyticsService == null)
            {
                TDebug.LogWarning("Analytics Service not initialized.");
                return;
            }

            _analyticsService.LogEvent(eventName, parameters);
        }

        #endregion
    }
}
