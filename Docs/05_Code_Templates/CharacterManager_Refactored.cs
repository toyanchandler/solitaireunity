using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Level;
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
            EventManager.InGameEvents.BeforeLevelLoaded += HandleBeforeLevelLoaded;
            EventManager.InGameEvents.LevelLoaded += HandleLevelLoaded;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.BeforeLevelLoaded -= HandleBeforeLevelLoaded;
            EventManager.InGameEvents.LevelLoaded -= HandleLevelLoaded;
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

        private void HandleBeforeLevelLoaded()
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

        private void HandleLevelLoaded(GameObject levelRoot)
        {
            if (_character == null)
            {
                TDebug.LogWarning("CharacterManager cannot position character because character is missing.");
                return;
            }

            if (levelRoot == null)
            {
                TDebug.LogWarning("CharacterManager received null level root.");
                return;
            }

            if (!levelRoot.TryGetComponent(out LevelReferenceHolder levelReferences))
            {
                TDebug.LogWarning($"Loaded level '{levelRoot.name}' has no LevelReferenceHolder.");
                return;
            }

            if (levelReferences.CharSpawnPoint == null)
            {
                if (_useZeroFallbackWhenSpawnPointMissing)
                {
                    _character.transform.position = Vector3.zero;
                    TDebug.LogWarning($"Level '{levelRoot.name}' has no CharSpawnPoint. Used Vector3.zero fallback.");
                    return;
                }

                TDebug.LogWarning($"Level '{levelRoot.name}' requires CharSpawnPoint.");
                return;
            }

            _character.transform.SetPositionAndRotation(
                levelReferences.CharSpawnPoint.position,
                levelReferences.CharSpawnPoint.rotation
            );
        }
    }
}
