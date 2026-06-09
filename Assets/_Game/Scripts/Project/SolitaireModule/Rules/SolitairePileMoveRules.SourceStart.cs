using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitairePileMoveRules
    {
        internal static class SourceStart
        {
            private static readonly SolitaireSourceStartRule[] Rules = CreateRules();

            public static bool TryEvaluate(
                SolitaireBoardState board,
                CardState movingCard,
                PileRef source,
                bool allowFoundationToTableau,
                out string reason)
            {
                SolitaireSourceStartRule rule = Rules[(int)source.Type];
                return rule != null
                    ? rule(board, movingCard, source, allowFoundationToTableau, out reason)
                    : Result.Reject("Unsupported source pile.", out reason);
            }

            private static SolitaireSourceStartRule[] CreateRules()
            {
                var rules = new SolitaireSourceStartRule[4];
                rules[(int)SolitairePileType.Waste] = Waste.CanStart;
                rules[(int)SolitairePileType.Foundation] = Foundation.CanStart;
                rules[(int)SolitairePileType.Tableau] = Tableau.CanStart;
                return rules;
            }

            internal static class Waste
            {
                public static bool CanStart(
                    SolitaireBoardState board,
                    CardState movingCard,
                    PileRef source,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    PileTop.IsTopCardOfSource(board, source, movingCard.Id)
                        ? Result.Accept(out reason)
                        : Result.Reject("Only top Waste card can move.", out reason);
            }

            internal static class Foundation
            {
                public static bool CanStart(
                    SolitaireBoardState board,
                    CardState movingCard,
                    PileRef source,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    !IsDragBackAllowed(allowFoundationToTableau)
                        ? Result.Reject("Foundation drag back is disabled.", out reason)
                        : TryStartTopCard(board, movingCard, source, out reason);

                public static bool IsDragBackAllowed(bool allowFoundationToTableau) =>
                    allowFoundationToTableau;

                public static bool TryStartTopCard(
                    SolitaireBoardState board,
                    CardState movingCard,
                    PileRef source,
                    out string reason) =>
                    PileTop.IsTopCardOfSource(board, source, movingCard.Id)
                        ? Result.Accept(out reason)
                        : Result.Reject("Only top Foundation card can move.", out reason);
            }

            internal static class Tableau
            {
                public static bool CanStart(
                    SolitaireBoardState board,
                    CardState movingCard,
                    PileRef source,
                    bool allowFoundationToTableau,
                    out string reason)
                {
                    FixedCardPileState sourcePile = board.GetPile(source);
                    int startIndex = sourcePile.IndexOf(movingCard.Id);

                    return startIndex < 0
                        ? Result.Reject("Card is not in source Tableau.", out reason)
                        : TryValidateDragSequence(board, sourcePile, startIndex, out reason);
                }

                public static bool TryValidateDragSequence(
                    SolitaireBoardState board,
                    FixedCardPileState sourcePile,
                    int startIndex,
                    out string reason)
                {
                    for (int index = startIndex; index < sourcePile.Count; index++)
                    {
                        if (!TryValidateSequenceCard(board, sourcePile, startIndex, index, out reason))
                            return false;
                    }

                    return Result.Accept(out reason);
                }

                public static bool TryValidateSequenceCard(
                    SolitaireBoardState board,
                    FixedCardPileState sourcePile,
                    int startIndex,
                    int index,
                    out string reason)
                {
                    CardState current = board.GetCard(sourcePile[index]);

                    if (!TableauBuild.IsFaceUp(current))
                        return Result.Reject("Dragged Tableau sequence contains a face-down card.", out reason);

                    if (index <= startIndex)
                        return Result.Accept(out reason);

                    return TryValidateSequenceLink(board, sourcePile, index, out reason);
                }

                public static bool TryValidateSequenceLink(
                    SolitaireBoardState board,
                    FixedCardPileState sourcePile,
                    int index,
                    out string reason)
                {
                    CardState previous = board.GetCard(sourcePile[index - 1]);
                    CardState current = board.GetCard(sourcePile[index]);

                    return TableauBuild.IsValidBuild(previous, current)
                        ? Result.Accept(out reason)
                        : Result.Reject("Dragged Tableau sequence is not internally legal.", out reason);
                }
            }
        }
    }
}
