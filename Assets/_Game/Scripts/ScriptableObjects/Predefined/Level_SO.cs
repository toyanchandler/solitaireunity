using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Predefined
{
    [CreateAssetMenu(fileName = "Level_SO", menuName = "ThisGame/Levels/LevelSO", order = 2)]
    public class Level_SO : ScriptableObject
    {
        #region Private Fields

        [SerializeField] private GameObject _levelPrefab;

        #endregion

        #region Properties

        public GameObject LevelPrefab => _levelPrefab;

        #endregion
    }
}
