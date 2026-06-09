using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireWinBridge : MonoBehaviour
    {
        [SerializeField] private SolitaireDeckController deckController;

        public void Initialize(SolitaireDeckController controller)
        {
            Unsubscribe();
            deckController = controller;
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (deckController != null)
                deckController.GameWon += HandleGameWon;
        }

        private void Unsubscribe()
        {
            if (deckController != null)
                deckController.GameWon -= HandleGameWon;
        }

        private void HandleGameWon()
        {
            EventManager.InGameEvents.LevelSuccess?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            deckController ??= GetComponent<SolitaireDeckController>();
        }
#endif
    }
}
