using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    internal static class SolitairePileLayoutLogic
    {
        internal enum PileCardRefreshMode
        {
            Instant = 0,
            AnimatedMove = 1,
            AnimatedFlip = 2
        }

        internal static class Visibility
        {
            public static bool ShouldShowEmptySlotVisual(int pileCount) => pileCount == 0;

            public static bool ShouldRenderCardInPile(SolitairePileType pileType, int pileCount, int cardIndex) =>
                pileType switch
                {
                    SolitairePileType.Stock or SolitairePileType.Foundation => cardIndex == pileCount - 1,
                    SolitairePileType.Waste => cardIndex >= pileCount - 3,
                    _ => true
                };

            public static bool ShouldRenderStockTopCard(SolitairePileType pileType, bool isTopStockCard) =>
                pileType == SolitairePileType.Stock && isTopStockCard;
        }

        internal static class Animation
        {
            public const float FlipMoveDistanceThreshold = 0.02f;
            public const float FlipArcHeightScale = 0.35f;

            internal enum FlipPresentationKind
            {
                MoveThenFlip = 0,
                FlipReveal = 1
            }

            public static bool ShouldLockInputForAnimation(bool animate, int pileCount) =>
                animate && pileCount > 0;

            public static float GetPileAnimationDuration(
                SolitaireDeckConfigSO config,
                bool animate,
                int flipCardId) =>
                !animate
                    ? 0f
                    : flipCardId >= 0
                        ? config.MoveAnimationDuration + config.FlipAnimationDuration
                        : config.MoveAnimationDuration;

            public static bool RequiresMoveBeforeFlip(Vector3 currentPosition, Vector3 targetPosition) =>
                Vector3.Distance(currentPosition, targetPosition) > FlipMoveDistanceThreshold;

            public static FlipPresentationKind ResolveFlipPresentationKind(
                Vector3 currentPosition,
                Vector3 targetPosition) =>
                RequiresMoveBeforeFlip(currentPosition, targetPosition)
                    ? FlipPresentationKind.MoveThenFlip
                    : FlipPresentationKind.FlipReveal;

            public static float GetFlipArcHeight(SolitaireDeckConfigSO config) =>
                config.DealArcHeight * FlipArcHeightScale;
        }

        internal static class Placement
        {
            public static bool IsCardInStock(SolitairePileType pileType) =>
                pileType == SolitairePileType.Stock;

            public static PileCardRefreshMode ResolveCardRefreshMode(
                bool animate,
                int cardId,
                int flipCardId,
                bool isFaceUp) =>
                !animate
                    ? PileCardRefreshMode.Instant
                    : cardId == flipCardId && isFaceUp
                        ? PileCardRefreshMode.AnimatedFlip
                        : PileCardRefreshMode.AnimatedMove;
        }

        internal static class Sorting
        {
            public static int GetCardSortingOrder(int baseSortingOrder, int pileIndex) =>
                baseSortingOrder + pileIndex;
        }
    }
}
