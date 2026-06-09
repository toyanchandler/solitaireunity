using _Game.Scripts.Managers.Core;

namespace _Game.Scripts.UI.Buttons
{
    public class RetryLevelButton : ButtonBase
    {
        protected override void OnClicked()
        {
            EventManager.InGameEvents.LoadLevel?.Invoke();
        }
    }
}
