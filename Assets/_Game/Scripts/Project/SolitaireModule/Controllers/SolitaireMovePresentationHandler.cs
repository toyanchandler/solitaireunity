using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireMovePresentationHandler
    {
        private readonly Action<SolitaireMove>[] _acceptedMoveFeedback = new Action<SolitaireMove>[4];

        private SolitaireLayoutController _layoutController;
        private SolitaireHapticFeedbackProvider _hapticFeedbackProvider;

        public void Initialize(
            SolitaireLayoutController layoutController,
            SolitaireHapticFeedbackProvider hapticFeedbackProvider)
        {
            _layoutController = layoutController;
            _hapticFeedbackProvider = hapticFeedbackProvider;
            BindAcceptedMoveFeedback();
        }

        public void HandleAcceptedMove(SolitaireMove move, SolitaireMoveResult result)
        {
            _layoutController.RefreshAll(true, result.RevealedCardId);

            if (result.RevealedCardId >= 0)
            {
                _hapticFeedbackProvider?.PlayMedium();
                return;
            }

            _acceptedMoveFeedback[(int)move.Target.Type]?.Invoke(move);
        }

        public void HandleFlipTableauTop(PileRef pile, int cardId)
        {
            _layoutController.RefreshPile(pile, true, cardId);
            _hapticFeedbackProvider?.PlayMedium();
        }

        public void HandleStockDraw(int drawnCardId)
        {
            _layoutController.RefreshPile(new PileRef(SolitairePileType.Stock, 0), true);
            _layoutController.RefreshPile(new PileRef(SolitairePileType.Waste, 0), true, drawnCardId);
            _hapticFeedbackProvider?.PlayLight();
        }

        public void HandleWasteRecycle()
        {
            _layoutController.RefreshAll(true);
            _hapticFeedbackProvider?.PlayLight();
        }

        public void HandleUndo()
        {
            _layoutController.RefreshAll(true);
        }

        public void HandleReturnToPile(int cardId, SolitaireBoardState board)
        {
            CardState card = board.GetCard(cardId);
            _layoutController.RefreshPile(new PileRef(card.CurrentPileType, card.CurrentPileIndex), true);
        }

        public void PlayInitialDeal(Action onCompleted)
        {
            _layoutController.PlayInitialDeal(onCompleted);
        }

        public void RefreshAll(bool animate)
        {
            _layoutController.RefreshAll(animate);
        }

        public void ApplyDebugScenarioPresentation()
        {
            _layoutController.CancelActivePresentations();
            _layoutController.RefreshAll(false);
        }

        public void PlayWinCelebration(Action onCompleted = null)
        {
            _layoutController.PlayWinCelebration(() =>
            {
                _hapticFeedbackProvider?.PlaySuccess();
                onCompleted?.Invoke();
            });
        }

        public void HandleInvalidMove()
        {
            _hapticFeedbackProvider?.PlayWarning();
        }

        private void BindAcceptedMoveFeedback()
        {
            _acceptedMoveFeedback[(int)SolitairePileType.Foundation] = PlayFoundationAcceptedFeedback;
            _acceptedMoveFeedback[(int)SolitairePileType.Tableau] = _ => _hapticFeedbackProvider?.PlayLight();
            _acceptedMoveFeedback[(int)SolitairePileType.Waste] = _ => _hapticFeedbackProvider?.PlayLight();
            _acceptedMoveFeedback[(int)SolitairePileType.Stock] = _ => _hapticFeedbackProvider?.PlayLight();
        }

        private void PlayFoundationAcceptedFeedback(SolitaireMove move)
        {
            _layoutController.PlayFoundationPulse(move.Target);
            _hapticFeedbackProvider?.PlayMedium();
        }
    }
}
