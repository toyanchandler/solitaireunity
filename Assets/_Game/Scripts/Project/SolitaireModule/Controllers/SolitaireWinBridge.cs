using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireWinBridge : MonoBehaviour
    {
        private void OnEnable()
        {
            EventManager.SolitaireEvents.GameWon += HandleGameWon;
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.GameWon -= HandleGameWon;
        }

        private void HandleGameWon()
        {
            EventManager.InGameEvents.LevelSuccess?.Invoke();
        }
    }
}
