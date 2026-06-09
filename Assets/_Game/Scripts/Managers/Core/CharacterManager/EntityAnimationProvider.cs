using System.Collections.Generic;
using _Game.Scripts.Helper.Extensions.System;
using Handler.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.Managers.Core.CharacterManager
{
    public sealed class EntityAnimationProvider : SerializedMonoBehaviour
    {
        #region Serialized Variables

        [SerializeField] private Animator animator;

        #endregion

        #region Private Variables

        [OdinSerialize] private Dictionary<GameState, string> gameStateAnimationData;
        private readonly Dictionary<GameState, int> _animationHashes = new Dictionary<GameState, int>();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ValidateReferences();
            AnimationDictHashing();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += HandleLevelLoaded;
            EventManager.InGameEvents.LevelStart += HandleLevelStart;
            EventManager.InGameEvents.LevelFail += HandleLevelFail;
            EventManager.InGameEvents.LevelSuccess += HandleLevelSuccess;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= HandleLevelLoaded;
            EventManager.InGameEvents.LevelStart -= HandleLevelStart;
            EventManager.InGameEvents.LevelFail -= HandleLevelFail;
            EventManager.InGameEvents.LevelSuccess -= HandleLevelSuccess;
        }

        #endregion

        #region Private Methods

        private void AnimationDictHashing()
        {
            _animationHashes.Clear();

            if (gameStateAnimationData == null)
            {
                return;
            }

            foreach (KeyValuePair<GameState, string> entry in gameStateAnimationData)
            {
                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    TDebug.LogWarning($"{nameof(EntityAnimationProvider)} has an empty trigger for state '{entry.Key}'.");
                    continue;
                }

                _animationHashes[entry.Key] = Animator.StringToHash(entry.Value);
            }
        }

        private void HandleLevelLoaded(GameObject go)
        {
            SwitchAnimation(GameState.LevelLoaded);
        }
        
        private void HandleLevelStart()
        {
            SwitchAnimation(GameState.LevelStart);
        }
        
        private void HandleLevelSuccess()
        {
            SwitchAnimation(GameState.Success);
        }

        private void HandleLevelFail()
        {
            SwitchAnimation(GameState.Fail);
        }

        private void SwitchAnimation(GameState state)
        {
            if (animator == null)
            {
                TDebug.LogWarning($"{nameof(EntityAnimationProvider)} cannot switch animation because Animator is missing.");
                return;
            }

            if (_animationHashes.TryGetValue(state, out int triggerHash))
            {
                animator.SetTrigger(triggerHash);
                return;
            }

            if (gameStateAnimationData != null && gameStateAnimationData.TryGetValue(state, out string triggerName))
            {
                TDebug.LogWarning($"{nameof(EntityAnimationProvider)} trigger '{triggerName}' for state '{state}' was not hashed.");
                return;
            }

            TDebug.LogWarning($"{nameof(EntityAnimationProvider)} has no animation trigger for state '{state}'.");
        }

        private void ValidateReferences()
        {
            if (animator == null)
            {
                TDebug.LogWarning($"{nameof(EntityAnimationProvider)} requires an Animator.");
            }

            if (gameStateAnimationData == null || gameStateAnimationData.Count == 0)
            {
                TDebug.LogWarning($"{nameof(EntityAnimationProvider)} requires game-state animation data.");
            }
        }

        #endregion
    }
}
