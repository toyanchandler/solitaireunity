using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitairePileMoveRules
    {
        internal static class TargetAccept
        {
            private static readonly SolitaireTargetAcceptRule[] Rules = CreateRules();

            public static bool TryEvaluate(
                SolitaireBoardState board,
                CardState movingCard,
                SolitaireMove move,
                out string reason)
            {
                SolitaireTargetAcceptRule rule = Rules[(int)move.Target.Type];
                return rule != null
                    ? rule(board, movingCard, move, out reason)
                    : Result.Reject("Unsupported target pile.", out reason);
            }

            private static SolitaireTargetAcceptRule[] CreateRules()
            {
                var rules = new SolitaireTargetAcceptRule[4];
                rules[(int)SolitairePileType.Tableau] = Tableau.CanAccept;
                rules[(int)SolitairePileType.Foundation] = Foundation.CanAccept;
                return rules;
            }

            internal static class Tableau
            {
                public static bool CanAccept(
                    SolitaireBoardState board,
                    CardState movingCard,
                    SolitaireMove move,
                    out string reason)
                {
                    FixedCardPileState targetPile = board.GetPile(move.Target);

                    return targetPile.Count == 0
                        ? TryAcceptOnEmpty(movingCard, out reason)
                        : TryAcceptOnOccupied(board, targetPile, movingCard, out reason);
                }

                public static bool TryAcceptOnEmpty(CardState movingCard, out string reason) =>
                    TableauBuild.IsKing(movingCard)
                        ? Result.Accept(out reason)
                        : Result.Reject("Only King can move to empty Tableau.", out reason);

                public static bool TryAcceptOnOccupied(
                    SolitaireBoardState board,
                    FixedCardPileState targetPile,
                    CardState movingCard,
                    out string reason)
                {
                    CardState targetTop = board.GetCard(targetPile.PeekTop());

                    return !TableauBuild.IsFaceUp(targetTop)
                        ? Result.Reject("Target Tableau top card is face-down.", out reason)
                        : TryAcceptOnFaceUpTop(targetTop, movingCard, out reason);
                }

                public static bool TryAcceptOnFaceUpTop(
                    CardState targetTop,
                    CardState movingCard,
                    out string reason) =>
                    TableauBuild.IsValidBuild(targetTop, movingCard)
                        ? Result.Accept(out reason)
                        : Result.Reject("Tableau target requires descending rank with opposite color.", out reason);
            }

            internal static class Foundation
            {
                public static bool CanAccept(
                    SolitaireBoardState board,
                    CardState movingCard,
                    SolitaireMove move,
                    out string reason) =>
                    !PileTop.IsTopCardOfSource(board, move.Source, movingCard.Id)
                        ? Result.Reject("Foundation accepts single top cards only.", out reason)
                        : TryAcceptOnFoundation(board, move, movingCard, out reason);

                public static bool TryAcceptOnFoundation(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    CardState movingCard,
                    out string reason)
                {
                    FixedCardPileState foundation = board.GetPile(move.Target);

                    return foundation.Count == 0
                        ? TryAcceptOnEmptyFoundation(movingCard, out reason)
                        : TryAcceptOnOccupiedFoundation(board, foundation, movingCard, out reason);
                }

                public static bool TryAcceptOnEmptyFoundation(CardState movingCard, out string reason) =>
                    FoundationBuild.CanStartEmptyFoundation(movingCard)
                        ? Result.Accept(out reason)
                        : Result.Reject("Foundation starts with Ace.", out reason);

                public static bool TryAcceptOnOccupiedFoundation(
                    SolitaireBoardState board,
                    FixedCardPileState foundation,
                    CardState movingCard,
                    out string reason)
                {
                    CardState top = board.GetCard(foundation.PeekTop());

                    return FoundationBuild.IsSameSuitAscending(top, movingCard)
                        ? Result.Accept(out reason)
                        : Result.Reject("Foundation requires same suit ascending rank.", out reason);
                }
            }
        }
    }
}
