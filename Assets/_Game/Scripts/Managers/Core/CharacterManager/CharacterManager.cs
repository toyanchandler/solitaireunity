using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.InGame.ReferenceHolder;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Managers.Core.CharacterManager
{
    public sealed class CharacterManager : MonoBehaviour
    {
        [SerializeField] private GameObject _character;
        [SerializeField] private bool _useZeroFallbackWhenSpawnPointMissing;

        private GameObject _characterMasterVariant;

        private void Awake()
        {
            ValidateReferences();
            CacheCharacterMasterVariant();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.BeforeLevelLoaded += HandleOnBeforeLoadLevel;
            EventManager.InGameEvents.LevelLoaded += HandleOnLevelLoaded;
        }
        
        private void OnDisable()
        {
            EventManager.InGameEvents.BeforeLevelLoaded -= HandleOnBeforeLoadLevel;
            EventManager.InGameEvents.LevelLoaded -= HandleOnLevelLoaded;
        }

        private void ValidateReferences()
        {
            if (_character == null)
            {
                TDebug.LogWarning($"{nameof(CharacterManager)} requires a character reference.");
            }
        }

        private void CacheCharacterMasterVariant()
        {
            if (_character == null || _character.transform.parent == null)
            {
                return;
            }

            _characterMasterVariant = _character.transform.parent.gameObject;
        }
        
        private void HandleOnBeforeLoadLevel()
        {
            if (_characterMasterVariant == null)
            {
                CacheCharacterMasterVariant();
            }
            
            if (_characterMasterVariant == null || _characterMasterVariant.activeSelf)
            {
                return;
            }

            _characterMasterVariant.SetActive(true);
            
            TDebug.LogGreen("Character Master Variant Activated");
        }

        private void HandleOnLevelLoaded(GameObject level)
        {
            if (_character == null)
            {
                TDebug.LogWarning("CharacterManager cannot position character because character is missing.");
                return;
            }

            if (level == null)
            {
                TDebug.LogWarning("CharacterManager received a null loaded level.");
                return;
            }

            if (!level.TryGetComponent(out LevelReferenceHolder levelReferenceHolder))
            {
                TDebug.LogWarning($"Loaded level '{level.name}' has no LevelReferenceHolder.");
                return;
            }

            if (!levelReferenceHolder.Validate(out string validationError))
            {
                if (_useZeroFallbackWhenSpawnPointMissing)
                {
                    _character.transform.position = Vector3.zero;
                    TDebug.LogWarning($"{validationError} Used Vector3.zero fallback because debug fallback is enabled.");
                    return;
                }

                TDebug.LogWarning(validationError);
                return;
            }

            Transform spawnPoint = levelReferenceHolder.CharSpawnPoint;
            _character.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }
    }
}
