using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using UnityEngine;

namespace _Game.Scripts.UI.Buttons
{
    public class NextLevelButton : ButtonBase
    {
        [SerializeField] PlayerSaveableData playerVariable;
        protected override void OnClicked()
        {
            playerVariable.AdvanceLevel();
            EventManager.InGameEvents.LoadLevel?.Invoke();
        }
    }
}
