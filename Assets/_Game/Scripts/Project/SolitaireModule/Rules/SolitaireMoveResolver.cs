using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    public sealed class SolitaireMoveResolver
    {
        private static readonly bool[] BypassCardValidationMoveTypes = CreateBypassCardValidationMoveTypes();

        public bool CanDrawFromStock(SolitaireBoardState board)
        {
            return board.Stock.Count > 0 || board.Waste.Count > 0;
        }

        public SolitaireMove ResolveDragMove(SolitaireBoardState board, int startCardId, PileRef target)
        {
            CardState card = board.GetCard(startCardId);
            var source = new PileRef(card.CurrentPileType, card.CurrentPileIndex);
            SolitaireMoveType moveType = SolitaireDragMoveLookup.Resolve(target.Type, source.Type);
            return new SolitaireMove(moveType, startCardId, source, target);
        }

        public SolitaireMove ResolveAutoFoundationMove(SolitaireBoardState board, int cardId)
        {
            CardState card = board.GetCard(cardId);
            var source = new PileRef(card.CurrentPileType, card.CurrentPileIndex);
            var target = ResolveFoundationTarget(board, card);
            return new SolitaireMove(SolitaireMoveType.AutoMoveToFoundation, cardId, source, target);
        }

        public bool CanStartMove(SolitaireBoardState board, int cardId, bool allowFoundationToTableau, out string reason)
        {
            CardState movingCard = board.GetCard(cardId);

            return movingCard.IsFaceUp
                ? SolitairePileMoveRules.TryCanStartFromSource(
                    board,
                    movingCard,
                    new PileRef(movingCard.CurrentPileType, movingCard.CurrentPileIndex),
                    allowFoundationToTableau,
                    out reason)
                : Reject("Face-down cards cannot move.", out reason);
        }

        public bool CanExecute(SolitaireBoardState board, SolitaireMove move, bool allowFoundationToTableau, out string reason)
        {
            if (move.Type == SolitaireMoveType.None || !move.Source.IsValid || !move.Target.IsValid)
                return Reject("Invalid move shape.", out reason);

            if (BypassCardValidationMoveTypes[(int)move.Type])
                return Accept(out reason);

            CardState movingCard = board.GetCard(move.StartCardId);

            if (!movingCard.IsFaceUp)
                return Reject("Face-down cards cannot move.", out reason);

            return SolitairePileMoveRules.TryCanStartFromSource(board, movingCard, move.Source, allowFoundationToTableau, out reason) &&
                   SolitairePileMoveRules.TryCanMoveToTarget(board, movingCard, move, out reason);
        }

        public bool CanFlipTableauTop(SolitaireBoardState board, PileRef tableau, int cardId, out string reason)
        {
            if (tableau.Type != SolitairePileType.Tableau)
                return Reject("Only Tableau top card can flip.", out reason);

            FixedCardPileState pile = board.GetPile(tableau);

            if (pile.Count == 0)
                return Reject("Tableau is empty.", out reason);

            int topCardId = pile.PeekTop();

            if (cardId >= 0 && topCardId != cardId)
                return Reject("Only top Tableau card can flip.", out reason);

            CardState topCard = board.GetCard(topCardId);

            return topCard.IsFaceUp
                ? Reject("Tableau top card is already face-up.", out reason)
                : Accept(out reason);
        }

        private static PileRef ResolveFoundationTarget(SolitaireBoardState board, CardState movingCard)
        {
            for (int i = 0; i < board.Foundations.Length; i++)
            {
                FixedCardPileState foundation = board.Foundations[i];

                if (foundation.Count <= 0)
                    continue;

                CardState top = board.GetCard(foundation.PeekTop());

                if (top.Suit == movingCard.Suit)
                    return new PileRef(SolitairePileType.Foundation, i);
            }

            if (movingCard.Rank == CardRank.Ace)
            {
                for (int i = 0; i < board.Foundations.Length; i++)
                {
                    if (board.Foundations[i].Count == 0)
                        return new PileRef(SolitairePileType.Foundation, i);
                }
            }

            return new PileRef(SolitairePileType.Foundation, SolitaireCardUtility.GetFoundationIndex(movingCard.Suit));
        }

        private static bool[] CreateBypassCardValidationMoveTypes()
        {
            var bypass = new bool[10];
            bypass[(int)SolitaireMoveType.StockToWaste] = true;
            bypass[(int)SolitaireMoveType.WasteRecycleToStock] = true;
            return bypass;
        }

        private static bool Accept(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        private static bool Reject(string message, out string reason)
        {
            reason = message;
            return false;
        }
    }
}
