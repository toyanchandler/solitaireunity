using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class GameStartHandler : MonoBehaviour
    {
        #region Unity Methods

        private void Start()
        {
            EventManager.InGameEvents.GameStarted?.Invoke();
        }

        #endregion
        
    }
}
