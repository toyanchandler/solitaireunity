using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal readonly struct CardLayoutMetrics
        {
            public CardLayoutMetrics(SolitaireDeckConfigSO config, Vector2 cardSize, float gap)
            {
                CardSize = cardSize;
                CardWidth = cardSize.x;
                CardHeight = cardSize.y;
                Gap = gap;
                CardScale = CardSizing.GetCardScale(CardWidth, config.CardSize.x);
            }

            public Vector2 CardSize { get; }
            public float CardWidth { get; }
            public float CardHeight { get; }
            public float Gap { get; }
            public float CardScale { get; }
        }

        internal static class CardSizing
        {
            public static float GetMaxCardWidthForRow(float availableWidth, int columnCount, float spacingRatio) =>
                availableWidth / (columnCount + ((columnCount - 1) * spacingRatio));

            public static float GetCardHeight(float cardWidth, float aspectRatio) =>
                cardWidth * aspectRatio;

            public static float GetCardGap(float cardWidth, float spacingRatio) =>
                cardWidth * spacingRatio;

            public static float GetCardScale(float cardWidth, float configCardWidth) =>
                cardWidth / Mathf.Max(LayoutConstants.MinCardSizeDivisor, configCardWidth);

            public static float GetMaxCardHeightFromVertical(
                float availableHeight,
                float totalVerticalGap,
                float verticalRowCount,
                float minTableauStackHeightFactor) =>
                (availableHeight - totalVerticalGap) / (verticalRowCount + minTableauStackHeightFactor);

            public static Vector2 FitToVerticalLimit(
                float maxCardHeight,
                float cardWidth,
                float cardHeight,
                float aspectRatio,
                float maxCardWidthForViewport,
                float maxCardWidthCap = float.MaxValue)
            {
                return ShouldClampToVerticalLimit(maxCardHeight, cardHeight)
                    ? ClampToVerticalLimit(maxCardHeight, aspectRatio, maxCardWidthForViewport, maxCardWidthCap)
                    : new Vector2(cardWidth, cardHeight);
            }

            public static bool ShouldClampToVerticalLimit(float maxCardHeight, float cardHeight) =>
                maxCardHeight > 0f && cardHeight > maxCardHeight;

            public static Vector2 ClampToVerticalLimit(
                float maxCardHeight,
                float aspectRatio,
                float maxCardWidthForViewport,
                float maxCardWidthCap)
            {
                float clampedHeight = maxCardHeight;
                float clampedWidth = Mathf.Min(clampedHeight / aspectRatio, maxCardWidthForViewport, maxCardWidthCap);
                float finalHeight = GetCardHeight(clampedWidth, aspectRatio);
                return new Vector2(clampedWidth, finalHeight);
            }
        }

        internal static class ScaledOffsets
        {
            public static float Scale(float configValue, float cardScale) =>
                configValue * cardScale;

            public static float GetTableauBottomPlayableY(float bottom, float cardHeight) =>
                bottom + (cardHeight * LayoutConstants.HalfCardCenterOffset);
        }

        internal static class RowLayout
        {
            public static float GetCenteredRowStartX(float centerX, int columnCount, float cardWidth, float gap)
            {
                float rowWidth = GetRowWidth(columnCount, cardWidth, gap);
                return centerX - (rowWidth * LayoutConstants.HalfCardCenterOffset) +
                       (cardWidth * LayoutConstants.HalfCardCenterOffset);
            }

            public static float GetRowWidth(int columnCount, float cardWidth, float gap) =>
                (columnCount * cardWidth) + ((columnCount - 1) * gap);

            public static float GetDistributedGap(float availableWidth, int columnCount, float cardWidth, float minimumGap) =>
                HasSingleColumn(columnCount)
                    ? minimumGap
                    : Mathf.Max(minimumGap, GetRawDistributedGap(availableWidth, columnCount, cardWidth));

            public static bool HasSingleColumn(int columnCount) =>
                columnCount <= 1;

            public static float GetRawDistributedGap(float availableWidth, int columnCount, float cardWidth) =>
                (availableWidth - (columnCount * cardWidth)) / (columnCount - 1);

            public static float GetRowStartX(float left, float cardWidth) =>
                left + (cardWidth * LayoutConstants.HalfCardCenterOffset);

            public static float GetRowCenterY(float rowTop, float cardHeight) =>
                rowTop - (cardHeight * LayoutConstants.HalfCardCenterOffset);

            public static float GetCardCenterX(float rowStartX, int index, float cardWidth, float gap) =>
                rowStartX + (index * (cardWidth + gap));

            public static Vector3[] BuildRowPositions(float rowStartX, int columnCount, float rowY, float cardWidth, float gap)
            {
                var positions = new Vector3[columnCount];

                for (int i = 0; i < columnCount; i++)
                {
                    float x = GetCardCenterX(rowStartX, i, cardWidth, gap);
                    positions[i] = new Vector3(x, rowY, 0f);
                }

                return positions;
            }
        }
    }
}
