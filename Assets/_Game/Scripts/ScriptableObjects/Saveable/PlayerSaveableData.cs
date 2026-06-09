using _Game.Scripts.ScriptableObjects.RunTime;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace _Game.Scripts.ScriptableObjects.Saveable
{
    [CreateAssetMenu(fileName = "PlayerVariable", menuName = "ThisGame/Player/PlayerVariable", order = 1)]
    public class PlayerSaveableData : PersistentSaveManager<PlayerSaveableData>, IResettable
    {
// Player stats and data
        #region PrivateFields
        
        [SerializeField] private int _levelIndex;

        #endregion

        #region PublicProperties

        public int LevelIndex => _levelIndex;

        public void SetLevelIndex(int levelIndex)
        {
            _levelIndex = Mathf.Max(0, levelIndex);
        }

        public void AdvanceLevel()
        {
            _levelIndex++;
        }

        #endregion
    }
}
