using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitaireMoveHandlerRegistry
    {
        internal static class Execution
        {
            internal static class Stock
            {
                public static SolitaireMoveResult Execute(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    bool autoFlipTableauTopCard)
                {
                    int cardId = board.Stock.RemoveTop();
                    return cardId < 0
                        ? SolitaireMoveResult.Rejected("Stock is empty.")
                        : AcceptAfterDraw(board, cardId);
                }

                private static SolitaireMoveResult AcceptAfterDraw(SolitaireBoardState board, int cardId)
                {
                    board.AddCardToPile(cardId, new PileRef(SolitairePileType.Waste, 0), true);
                    board.RefreshPileIndices(new PileRef(SolitairePileType.Stock, 0));
                    return SolitaireMoveResult.Accepted();
                }
            }

            internal static class WasteRecycle
            {
                public static SolitaireMoveResult Execute(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    bool autoFlipTableauTopCard) =>
                    board.Waste.Count == 0
                        ? SolitaireMoveResult.Rejected("Waste is empty.")
                        : AcceptAfterRecycle(board);

                private static SolitaireMoveResult AcceptAfterRecycle(SolitaireBoardState board)
                {
                    while (board.Waste.Count > 0)
                    {
                        int cardId = board.Waste.RemoveTop();
                        board.AddCardToPile(cardId, new PileRef(SolitairePileType.Stock, 0), false);
                    }

                    board.RefreshPileIndices(new PileRef(SolitairePileType.Waste, 0));
                    return SolitaireMoveResult.Accepted();
                }
            }

            internal static class CardTransfer
            {
                public readonly struct MovingCards
                {
                    public MovingCards(int[] cardIds, int count)
                    {
                        CardIds = cardIds;
                        Count = count;
                    }

                    public int[] CardIds { get; }
                    public int Count { get; }
                }

                public static SolitaireMoveResult Execute(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    bool autoFlipTableauTopCard)
                {
                    if (!TryExtractMovingCards(board, move, out MovingCards movingCards))
                        return SolitaireMoveResult.Rejected("Start card is not in source pile.");

                    ApplyTransfer(board, move, movingCards);
                    int revealedCardId = Reveal.ResolveAfterTransfer(board, move, autoFlipTableauTopCard);
                    return SolitaireMoveResult.Accepted(revealedCardId);
                }

                public static bool TryExtractMovingCards(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    out MovingCards movingCards)
                {
                    FixedCardPileState source = board.GetPile(move.Source);
                    int startIndex = source.IndexOf(move.StartCardId);

                    if (startIndex < 0)
                    {
                        movingCards = default;
                        return false;
                    }

                    int[] cardIds = new int[source.Count - startIndex];
                    source.CopyRangeTo(startIndex, cardIds, out int movingCount);
                    source.RemoveFromIndex(startIndex);
                    movingCards = new MovingCards(cardIds, movingCount);
                    return true;
                }

                public static void ApplyTransfer(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    MovingCards movingCards)
                {
                    for (int i = 0; i < movingCards.Count; i++)
                        board.AddCardToPile(movingCards.CardIds[i], move.Target, true);

                    board.RefreshPileIndices(move.Source);
                    board.RefreshPileIndices(move.Target);
                }
            }

            internal static class Reveal
            {
                public static int ResolveAfterTransfer(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    bool autoFlipTableauTopCard) =>
                    ShouldRevealTableauTop(move, autoFlipTableauTopCard)
                        ? TryFlipTableauTopFaceDown(board, move.Source)
                        : -1;

                public static bool ShouldRevealTableauTop(SolitaireMove move, bool autoFlipTableauTopCard) =>
                    autoFlipTableauTopCard && move.Source.Type == SolitairePileType.Tableau;

                public static int TryFlipTableauTopFaceDown(SolitaireBoardState board, PileRef source)
                {
                    FixedCardPileState sourceAfterMove = board.GetPile(source);
                    int topCardId = sourceAfterMove.PeekTop();

                    if (topCardId < 0)
                        return -1;

                    ref CardState topCard = ref board.GetCardRef(topCardId);

                    if (topCard.IsFaceUp)
                        return -1;

                    topCard.IsFaceUp = true;
                    return topCardId;
                }
            }

            internal static class FlipTableau
            {
                public static SolitaireMoveResult Execute(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    bool autoFlipTableauTopCard)
                {
                    if (!IsTableauSource(move.Source))
                        return SolitaireMoveResult.Rejected("Only Tableau top card can flip.");

                    int topCardId = board.GetPile(move.Source).PeekTop();

                    if (topCardId < 0)
                        return SolitaireMoveResult.Rejected("Tableau is empty.");

                    if (!MatchesRequestedCard(move.StartCardId, topCardId))
                        return SolitaireMoveResult.Rejected("Only top Tableau card can flip.");

                    return AcceptAfterFlip(board, topCardId);
                }

                public static bool IsTableauSource(PileRef source) =>
                    source.Type == SolitairePileType.Tableau;

                public static bool MatchesRequestedCard(int requestedCardId, int topCardId) =>
                    requestedCardId < 0 || topCardId == requestedCardId;

                private static SolitaireMoveResult AcceptAfterFlip(SolitaireBoardState board, int topCardId)
                {
                    ref CardState card = ref board.GetCardRef(topCardId);
                    card.IsFaceUp = true;
                    return SolitaireMoveResult.Accepted(topCardId);
                }
            }
        }
    }
}
