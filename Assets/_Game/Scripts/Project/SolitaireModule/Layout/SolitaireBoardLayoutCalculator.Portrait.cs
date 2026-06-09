using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal static class Portrait
        {
            internal readonly struct RowHeights
            {
                public RowHeights(float stockWasteRowY, float cardHeight, float rowGap)
                {
                    StockWasteRowY = stockWasteRowY;
                    FoundationRowY = stockWasteRowY - cardHeight - rowGap;
                    TableauY = FoundationRowY - cardHeight - rowGap;
                }

                public float StockWasteRowY { get; }
                public float FoundationRowY { get; }
                public float TableauY { get; }
            }

            internal readonly struct RowStarts
            {
                public RowStarts(float centerX, float cardWidth, float gap)
                {
                    StockWasteStartX = RowLayout.GetCenteredRowStartX(
                        centerX,
                        LayoutConstants.StockWasteColumnCount,
                        cardWidth,
                        gap);
                    FoundationStartX = RowLayout.GetCenteredRowStartX(
                        centerX,
                        SolitaireCardUtility.FoundationCount,
                        cardWidth,
                        gap);
                    TableauStartX = RowLayout.GetCenteredRowStartX(
                        centerX,
                        SolitaireCardUtility.TableauCount,
                        cardWidth,
                        gap);
                }

                public float StockWasteStartX { get; }
                public float FoundationStartX { get; }
                public float TableauStartX { get; }
            }

            public static SolitaireBoardLayoutResult Calculate(
                SolitaireDeckConfigSO config,
                Vector3 cameraPosition,
                ViewportBounds bounds)
            {
                Vector2 cardSize = CalculateCardSize(config, bounds);
                CardLayoutMetrics metrics = new CardLayoutMetrics(
                    config,
                    cardSize,
                    CardSizing.GetCardGap(cardSize.x, config.CardHorizontalSpacingRatio));
                RowHeights rows = BuildRowHeights(bounds, metrics, config.RowVerticalGap);
                RowStarts starts = new RowStarts(cameraPosition.x, metrics.CardWidth, metrics.Gap);

                Vector3[] foundationPositions = RowLayout.BuildRowPositions(
                    starts.FoundationStartX,
                    SolitaireCardUtility.FoundationCount,
                    rows.FoundationRowY,
                    metrics.CardWidth,
                    metrics.Gap);
                Vector3[] tableauPositions = RowLayout.BuildRowPositions(
                    starts.TableauStartX,
                    SolitaireCardUtility.TableauCount,
                    rows.TableauY,
                    metrics.CardWidth,
                    metrics.Gap);

                return ResultFactory.Create(
                    config,
                    metrics.CardSize,
                    metrics.CardScale,
                    bounds.Bottom,
                    starts.StockWasteStartX,
                    rows.StockWasteRowY,
                    metrics.CardWidth,
                    metrics.Gap,
                    foundationPositions,
                    tableauPositions);
            }

            public static Vector2 CalculateCardSize(SolitaireDeckConfigSO config, ViewportBounds bounds)
            {
                float spacingRatio = config.CardHorizontalSpacingRatio;
                float maxCardWidthForViewport = CardSizing.GetMaxCardWidthForRow(
                    bounds.AvailableWidth,
                    SolitaireCardUtility.TableauCount,
                    spacingRatio);
                float cardWidth = Mathf.Min(maxCardWidthForViewport, config.MaxResponsiveCardWidth);
                float cardHeight = CardSizing.GetCardHeight(cardWidth, config.CardAspectRatio);
                float maxCardHeightFromVertical = CardSizing.GetMaxCardHeightFromVertical(
                    bounds.AvailableHeight,
                    config.RowVerticalGap * LayoutConstants.PortraitVerticalGapMultiplier,
                    LayoutConstants.PortraitVerticalRowCount,
                    LayoutConstants.PortraitMinTableauStackHeightFactor);

                return CardSizing.FitToVerticalLimit(
                    maxCardHeightFromVertical,
                    cardWidth,
                    cardHeight,
                    config.CardAspectRatio,
                    maxCardWidthForViewport);
            }

            private static RowHeights BuildRowHeights(ViewportBounds bounds, CardLayoutMetrics metrics, float rowGap)
            {
                float stockWasteRowY = RowLayout.GetRowCenterY(bounds.Top, metrics.CardHeight);
                return new RowHeights(stockWasteRowY, metrics.CardHeight, rowGap);
            }
        }
    }
}
