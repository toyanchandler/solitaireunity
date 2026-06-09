using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Helper.Extensions.System;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Predefined
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "ThisGame/Levels/LevelList", order = 1)]
    public class LevelList_SO : ScriptableObject
    {
        #region Private Variables
        [SerializeField]
        private List<Level_SO> _allLevels;
        #endregion

        #region Public Methods
        public Level_SO GetLevelWithIndex(int currentLevel)
        {
            // Ensure the list is not empty to avoid division by zero
            if (_allLevels.Count == 0)
            {
                TDebug.LogWarning("The level list is empty.");
                return null;
            }

            // Use modulus to loop back to the start of the list after reaching the end
            int index = currentLevel % _allLevels.Count;

            // Retrieve the level at the calculated index
            var level = _allLevels[index];
        
            return level;
        }
        #endregion
    }
}
