using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitairePileMoveRules
    {
        internal static class PileTop
        {
            public static bool IsTopCard(FixedCardPileState pile, int cardId) =>
                pile.IsTopCard(cardId);

            public static bool IsTopCardOfSource(SolitaireBoardState board, PileRef source, int cardId) =>
                IsTopCard(board.GetPile(source), cardId);
        }

        internal static class TableauBuild
        {
            public static bool IsValidBuild(CardState upper, CardState lower) =>
                IsDescendingRank(upper, lower) && HasOppositeColor(upper, lower);

            public static bool IsDescendingRank(CardState upper, CardState lower) =>
                (int)lower.Rank == (int)upper.Rank - 1;

            public static bool HasOppositeColor(CardState upper, CardState lower) =>
                SolitaireCardUtility.HasOppositeColor(upper, lower);

            public static bool IsKing(CardState card) =>
                card.Rank == CardRank.King;

            public static bool IsAce(CardState card) =>
                card.Rank == CardRank.Ace;

            public static bool IsFaceUp(CardState card) =>
                card.IsFaceUp;
        }

        internal static class FoundationBuild
        {
            public static bool IsSameSuitAscending(CardState top, CardState moving) =>
                top.Suit == moving.Suit && (int)moving.Rank == (int)top.Rank + 1;

            public static bool CanStartEmptyFoundation(CardState movingCard) =>
                TableauBuild.IsAce(movingCard);
        }
    }
}
