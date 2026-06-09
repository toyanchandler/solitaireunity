using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    internal static class SolitaireInitialDealLogic
    {
        internal const float InputLockSafetyPaddingSeconds = 0.1f;
        internal const int ColumnSortingStride = 4;

        internal static class Runtime
        {
            public static bool IsReady(
                SolitaireDeckConfigSO config,
                SolitaireRuntimeContext context,
                SolitairePileLayoutPresenter pileLayoutPresenter) =>
                config != null && context != null && pileLayoutPresenter != null;
        }

        internal static class DealSteps
        {
            public static int CountForTableau(int tableauCount) =>
                tableauCount * (tableauCount + 1) / 2;

            public static int CountForDefaultTableau() =>
                CountForTableau(SolitaireCardUtility.TableauCount);
        }

        internal static class Timing
        {
            public static float CalculateInputLockDuration(
                int dealStepCount,
                float dealStaggerDelay,
                float dealAnimationDuration,
                float flipAnimationDuration,
                float safetyPaddingSeconds = InputLockSafetyPaddingSeconds) =>
                (dealStepCount * dealStaggerDelay) +
                (dealAnimationDuration + flipAnimationDuration) +
                safetyPaddingSeconds;

            public static float CalculatePostDealWaitDuration(
                float dealAnimationDuration,
                float flipAnimationDuration) =>
                dealAnimationDuration + flipAnimationDuration;
        }

        internal static class DealIteration
        {
            public static bool IsValidTableauColumn(int columnIndex) =>
                columnIndex >= 0 && columnIndex < SolitaireCardUtility.TableauCount;

            public static bool IsValidRowForColumn(int rowIndex, int columnIndex) =>
                rowIndex >= 0 && rowIndex <= columnIndex;
        }

        internal static class TableauColumn
        {
            public static int GetInclusiveRowCount(int columnIndex) => columnIndex + 1;

            public static bool ShouldFlipOnLand(int rowIndex, int columnIndex) =>
                rowIndex == columnIndex;

            public static bool ShouldShowSlotVisual(int pileCardCount) =>
                pileCardCount == 0;

            public static int CalculateSortingOrder(int baseSortingOrder, int rowIndex, int columnIndex) =>
                baseSortingOrder + rowIndex + (columnIndex * ColumnSortingStride);

            public static PileRef CreatePileRef(int columnIndex) =>
                new PileRef(SolitairePileType.Tableau, columnIndex);
        }

        internal static class PostDealRefresh
        {
            public static PileRef Stock => new PileRef(SolitairePileType.Stock, 0);

            public static PileRef Waste => new PileRef(SolitairePileType.Waste, 0);

            public static PileRef CreateFoundationPileRef(int foundationIndex) =>
                new PileRef(SolitairePileType.Foundation, foundationIndex);

            public static PileRef CreateTableauPileRef(int tableauIndex) =>
                new PileRef(SolitairePileType.Tableau, tableauIndex);
        }
    }
}
