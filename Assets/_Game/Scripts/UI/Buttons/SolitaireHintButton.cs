using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.UI.Buttons
{
    public sealed class SolitaireHintButton : ButtonBase
    {
        private SolitaireDeckController _deckController;

        private void OnEnable()
        {
            TryResolveDeckController();
        }

        protected override void OnClicked()
        {
            TryResolveDeckController();
            _deckController?.TryShowNextHint();
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
