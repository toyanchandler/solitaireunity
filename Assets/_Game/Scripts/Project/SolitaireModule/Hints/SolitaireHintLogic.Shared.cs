using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class PileRefs
        {
            public static PileRef Waste() => new PileRef(SolitairePileType.Waste, 0);

            public static PileRef Stock() => new PileRef(SolitairePileType.Stock, 0);

            public static PileRef Tableau(int index) => new PileRef(SolitairePileType.Tableau, index);
        }

        internal static class CardQueries
        {
            public static bool HasTopCard(int cardId) => cardId >= 0;

            public static bool IsFaceUp(CardState card) => card.IsFaceUp;

            public static bool IsHidden(CardState card) => !card.IsFaceUp;
        }

        internal static class RevealDetection
        {
            public static bool IsTableauPile(FixedCardPileState pile) =>
                pile.PileType == SolitairePileType.Tableau;

            public static bool HasCardBelow(int cardIndex) => cardIndex > 0;

            public static bool WillRevealHiddenTableauCard(
                SolitaireBoardState board,
                FixedCardPileState sourcePile,
                int movingCardIndex) =>
                IsTableauPile(sourcePile) &&
                HasCardBelow(movingCardIndex) &&
                CardQueries.IsHidden(board.GetCard(sourcePile[movingCardIndex - 1]));
        }

        internal static class TableauMoveFilter
        {
            public static bool IsDifferentColumn(int sourceIndex, int targetIndex) =>
                targetIndex != sourceIndex;

            public static bool ShouldInclude(bool revealOnly, bool revealsHiddenCard) =>
                revealOnly ? revealsHiddenCard : !revealsHiddenCard;
        }

        internal static class HintKindResolution
        {
            public static SolitaireHintKind ForTableauToTableau(bool revealsHiddenCard) =>
                revealsHiddenCard
                    ? SolitaireHintKind.RevealTableauByMove
                    : SolitaireHintKind.TableauToTableau;
        }

        internal static class MoveFactory
        {
            public static SolitaireMove CreateWasteToTableau(int cardId, int targetIndex) =>
                new SolitaireMove(
                    SolitaireMoveType.WasteToTableau,
                    cardId,
                    PileRefs.Waste(),
                    PileRefs.Tableau(targetIndex));

            public static SolitaireMove CreateTableauToTableau(int cardId, int sourceIndex, int targetIndex) =>
                new SolitaireMove(
                    SolitaireMoveType.TableauToTableau,
                    cardId,
                    PileRefs.Tableau(sourceIndex),
                    PileRefs.Tableau(targetIndex));

            public static SolitaireMove CreateStockToWaste() =>
                new SolitaireMove(
                    SolitaireMoveType.StockToWaste,
                    -1,
                    PileRefs.Stock(),
                    PileRefs.Waste());

            public static SolitaireMove CreateWasteRecycleToStock() =>
                new SolitaireMove(
                    SolitaireMoveType.WasteRecycleToStock,
                    -1,
                    PileRefs.Waste(),
                    PileRefs.Stock());
        }

        internal static class MoveEquality
        {
            public static bool AreSamePileRef(PileRef a, PileRef b) =>
                a.Type == b.Type && a.Index == b.Index;

            public static bool AreSameMove(SolitaireMove a, SolitaireMove b) =>
                a.Type == b.Type &&
                a.StartCardId == b.StartCardId &&
                AreSamePileRef(a.Source, b.Source) &&
                AreSamePileRef(a.Target, b.Target);
        }

        internal static class HintCollection
        {
            public static bool HasCapacity(int count, int capacity) =>
                count < capacity;

            public static bool IsDuplicate(SolitaireHint[] target, int count, SolitaireMove move)
            {
                for (int i = 0; i < count; i++)
                {
                    if (MoveEquality.AreSameMove(target[i].Move, move))
                        return true;
                }

                return false;
            }

            public static bool CanAppend(SolitaireHint hint, SolitaireHint[] target, int count) =>
                hint.IsValid &&
                HasCapacity(count, target.Length) &&
                !IsDuplicate(target, count, hint.Move);

            public static bool TryAppend(SolitaireHint[] target, ref int count, SolitaireHint hint)
            {
                if (!CanAppend(hint, target, count))
                    return false;

                target[count] = hint;
                count++;
                return true;
            }

            public static void AppendIfUnique(SolitaireHint[] target, ref int count, SolitaireHint hint) =>
                TryAppend(target, ref count, hint);
        }

        internal static class Execution
        {
            public static bool CanExecute(
                SolitaireBoardState board,
                SolitaireMoveResolver resolver,
                SolitaireDeckConfigSO config,
                SolitaireMove move) =>
                resolver.CanExecute(board, move, config.AllowFoundationToTableau, out _);
        }

        internal static class HintResults
        {
            public static bool Fail(out SolitaireHint hint)
            {
                hint = SolitaireHint.None;
                return false;
            }
        }
    }
}
