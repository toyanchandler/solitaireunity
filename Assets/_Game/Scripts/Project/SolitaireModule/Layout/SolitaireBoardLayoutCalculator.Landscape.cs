using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal static class Landscape
        {
            internal readonly struct LandscapeTopRowLayout
            {
                public LandscapeTopRowLayout(float stockX, float wasteX, float foundationStartX, float foundationGap)
                {
                    StockX = stockX;
                    WasteX = wasteX;
                    FoundationStartX = foundationStartX;
                    FoundationGap = foundationGap;
                }

                public float StockX { get; }
                public float WasteX { get; }
                public float FoundationStartX { get; }
                public float FoundationGap { get; }
            }

            public static SolitaireBoardLayoutResult Calculate(SolitaireDeckConfigSO config, ViewportBounds bounds)
            {
                Vector2 cardSize = CalculateCardSize(config, bounds);
                CardLayoutMetrics metrics = new CardLayoutMetrics(
                    config,
                    cardSize,
                    CardSizing.GetCardGap(cardSize.x, config.CardHorizontalSpacingRatio));

                float tableauGap = RowLayout.GetDistributedGap(
                    bounds.AvailableWidth,
                    SolitaireCardUtility.TableauCount,
                    metrics.CardWidth,
                    metrics.Gap);
                float foundationGap = GetFoundationGap(metrics.Gap, tableauGap, metrics.CardWidth);
                float topRowY = RowLayout.GetRowCenterY(bounds.Top, metrics.CardHeight);
                float tableauY = CalculateTableauY(bounds, metrics.CardHeight, config.RowVerticalGap, topRowY);

                Vector3[] tableauPositions = BuildTableauPositions(bounds.Left, metrics.CardWidth, tableauGap, tableauY);
                LandscapeTopRowLayout topRow = ResolveTopRowLayout(bounds, metrics.CardWidth, metrics.Gap, foundationGap);
                Vector3[] foundationPositions = RowLayout.BuildRowPositions(
                    topRow.FoundationStartX,
                    SolitaireCardUtility.FoundationCount,
                    topRowY,
                    metrics.CardWidth,
                    topRow.FoundationGap);

                return ResultFactory.Create(
                    config,
                    metrics.CardSize,
                    metrics.CardScale,
                    bounds.Bottom,
                    topRow.StockX,
                    topRowY,
                    metrics.CardWidth,
                    metrics.Gap,
                    foundationPositions,
                    tableauPositions,
                    topRow.WasteX);
            }

            public static Vector2 CalculateCardSize(SolitaireDeckConfigSO config, ViewportBounds bounds)
            {
                float spacingRatio = config.CardHorizontalSpacingRatio;
                float maxCardWidthForTableau = CardSizing.GetMaxCardWidthForRow(
                    bounds.AvailableWidth,
                    SolitaireCardUtility.TableauCount,
                    spacingRatio);
                float maxCardWidthForTopRow = bounds.AvailableWidth /
                                               (LayoutConstants.LandscapeTopRowCardCount +
                                                (LayoutConstants.LandscapeFoundationGapCount * spacingRatio) +
                                                LayoutConstants.LandscapeTopGroupGapRatio);
                float maxCardWidthForViewport = Mathf.Min(maxCardWidthForTableau, maxCardWidthForTopRow);
                float cardWidth = Mathf.Min(maxCardWidthForViewport, LayoutConstants.LandscapeMaxCardWidth);
                float cardHeight = CardSizing.GetCardHeight(cardWidth, config.CardAspectRatio);
                float maxCardHeightFromVertical = CardSizing.GetMaxCardHeightFromVertical(
                    bounds.AvailableHeight,
                    config.RowVerticalGap,
                    LayoutConstants.LandscapeVerticalRowCount,
                    LayoutConstants.LandscapeMinTableauStackHeightFactor);

                return CardSizing.FitToVerticalLimit(
                    maxCardHeightFromVertical,
                    cardWidth,
                    cardHeight,
                    config.CardAspectRatio,
                    maxCardWidthForViewport,
                    LayoutConstants.LandscapeMaxCardWidth);
            }

            public static float GetFoundationGap(float compactGap, float tableauGap, float cardWidth) =>
                Mathf.Max(compactGap, Mathf.Min(tableauGap, cardWidth * LayoutConstants.LandscapeFoundationGapMaxRatio));

            public static float CalculateTableauY(
                ViewportBounds bounds,
                float cardHeight,
                float rowGap,
                float topRowY)
            {
                float naturalTableauY = topRowY - cardHeight - rowGap;
                float lowerTableauY = bounds.Bottom + (cardHeight * LayoutConstants.LandscapeTableauVerticalAnchor);
                return Mathf.Lerp(
                    naturalTableauY,
                    lowerTableauY,
                    LayoutConstants.LandscapeTableauVerticalBlend);
            }

            public static Vector3[] BuildTableauPositions(float left, float cardWidth, float tableauGap, float tableauY)
            {
                var tableauPositions = new Vector3[SolitaireCardUtility.TableauCount];

                for (int i = 0; i < tableauPositions.Length; i++)
                {
                    float x = RowLayout.GetRowStartX(left, cardWidth) + (i * (cardWidth + tableauGap));
                    tableauPositions[i] = new Vector3(x, tableauY, 0f);
                }

                return tableauPositions;
            }

            public static LandscapeTopRowLayout ResolveTopRowLayout(
                ViewportBounds bounds,
                float cardWidth,
                float compactGap,
                float foundationGap)
            {
                float stockX = RowLayout.GetRowStartX(bounds.Left, cardWidth);
                float wasteX = stockX + cardWidth + compactGap;
                float foundationStartX = GetRightAlignedFoundationStartX(bounds.Right, cardWidth, foundationGap);

                return TopRow.DoGroupsOverlap(wasteX, foundationStartX, cardWidth, compactGap)
                    ? TopRow.CreateOverlapLayout(bounds, cardWidth, compactGap)
                    : new LandscapeTopRowLayout(stockX, wasteX, foundationStartX, foundationGap);
            }

            public static float GetRightAlignedFoundationStartX(float right, float cardWidth, float foundationGap)
            {
                float foundationRowWidth = RowLayout.GetRowWidth(
                    SolitaireCardUtility.FoundationCount,
                    cardWidth,
                    foundationGap);
                return right - foundationRowWidth + (cardWidth * LayoutConstants.HalfCardCenterOffset);
            }

            internal static class TopRow
            {
                public static bool DoGroupsOverlap(
                    float wasteX,
                    float foundationStartX,
                    float cardWidth,
                    float compactGap)
                {
                    float minimumGroupGap = Mathf.Max(compactGap, cardWidth * LayoutConstants.LandscapeTopGroupGapRatio);
                    float wasteRightEdge = wasteX + (cardWidth * LayoutConstants.HalfCardCenterOffset);
                    float foundationLeftEdge = foundationStartX - (cardWidth * LayoutConstants.HalfCardCenterOffset);
                    return wasteRightEdge + minimumGroupGap > foundationLeftEdge;
                }

                public static LandscapeTopRowLayout CreateOverlapLayout(
                    ViewportBounds bounds,
                    float cardWidth,
                    float compactGap)
                {
                    float topGap = RowLayout.GetDistributedGap(
                        bounds.AvailableWidth,
                        LayoutConstants.LandscapeTopGroupOverlapColumnCount,
                        cardWidth,
                        compactGap);
                    float stockX = RowLayout.GetRowStartX(bounds.Left, cardWidth);
                    float wasteX = stockX + cardWidth + topGap;
                    float foundationStartX = wasteX + cardWidth + topGap;
                    return new LandscapeTopRowLayout(stockX, wasteX, foundationStartX, topGap);
                }
            }
        }
    }
}
