using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using Handler.Extensions;

namespace _Game.Scripts.UI.Buttons
{
    public class LevelStartButton : ButtonBase
    {
        protected override void OnClicked()
        {
            TDebug.Log("LevelStartButton Clicked");
            EventManager.InGameEvents.LevelStart?.Invoke();
        }
    }   

}
