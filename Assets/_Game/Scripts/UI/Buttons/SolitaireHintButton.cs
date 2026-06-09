using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.UI.Buttons
{
    public sealed class SolitaireHintButton : ButtonBase
    {
        private SolitaireDeckController _deckController;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelStart += HandleLevelStart;
            EventManager.SolitaireEvents.ControllerHostReady += HandleControllerHostReady;
            TryResolveDeckController();
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelStart -= HandleLevelStart;
            EventManager.SolitaireEvents.ControllerHostReady -= HandleControllerHostReady;
        }

        protected override void OnClicked()
        {
            TryResolveDeckController();

            if (_deckController == null)
            {
                Debug.LogWarning("[SolitaireHint] temp — DeckController missing; HINT click ignored.");
                return;
            }

            _deckController.TryShowNextHint();
        }

        private void HandleLevelStart()
        {
            TryResolveDeckController();
        }

        private void HandleControllerHostReady(SolitaireModuleControllerBundle bundle)
        {
            if (bundle == null)
                return;

            _deckController = bundle.DeckController;
        }

        private void TryResolveDeckController()
        {
            if (_deckController != null)
                return;

            if (SolitaireFeatureRegistration.TryGetControllerHost(out SolitaireModuleControllerBundle bundle, out _))
                _deckController = bundle.DeckController;
        }
    }
}
