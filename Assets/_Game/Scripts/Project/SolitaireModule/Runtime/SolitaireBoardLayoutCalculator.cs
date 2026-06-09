using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireBoardLayoutResult
    {
        public Vector2 CardSize;
        public float TableauBottomPlayableY;
        public float WasteStackXOffset;
        public float FaceDownTableauYOffset;
        public float FaceUpTableauYOffset;
        public float MinCompressedFaceUpYOffset;
        public float CardScale;
        public Vector3 StockPosition;
        public Vector3 WastePosition;
        public Vector3[] FoundationPositions;
        public Vector3[] TableauPositions;
    }

    public static class SolitaireBoardLayoutCalculator
    {
        private const float PortraitMinTableauStackHeightFactor = 3.5f;
        private const float LandscapeMinTableauStackHeightFactor = 0.85f;
        private const float LandscapeTopGroupGapRatio = 0.85f;
        private const float LandscapeMaxCardWidth = 1.45f;
        private const float LandscapeTopPadding = 0.95f;
        private const float LandscapeBottomPadding = 0.25f;

        public static SolitaireBoardLayoutResult CreateFromConfig(SolitaireDeckConfigSO config)
        {
            return new SolitaireBoardLayoutResult
            {
                CardSize = config.CardSize,
                TableauBottomPlayableY = config.TableauBottomPlayableY,
                WasteStackXOffset = config.WasteStackXOffset,
                FaceDownTableauYOffset = config.FaceDownTableauYOffset,
                FaceUpTableauYOffset = config.FaceUpTableauYOffset,
                MinCompressedFaceUpYOffset = config.MinCompressedFaceUpYOffset,
                CardScale = 1f,
                StockPosition = Vector3.zero,
                WastePosition = Vector3.zero,
                FoundationPositions = new Vector3[SolitaireCardUtility.FoundationCount],
                TableauPositions = new Vector3[SolitaireCardUtility.TableauCount],
            };
        }

        public static bool TryCalculateResponsive(
            Camera camera,
            SolitaireDeckConfigSO config,
            out SolitaireBoardLayoutResult result)
        {
            result = default;

            if (camera == null || !camera.orthographic || config == null)
                return false;

            GetLayoutFrustum(camera, out float halfWidth, out float halfHeight);
            Vector3 cameraPosition = camera.transform.position;
            float left = cameraPosition.x - halfWidth + config.BoardHorizontalPadding;
            float right = cameraPosition.x + halfWidth - config.BoardHorizontalPadding;
            int pixelWidth = Mathf.Max(1, camera.pixelWidth);
            int pixelHeight = Mathf.Max(1, camera.pixelHeight);
            bool isLandscape = pixelWidth > pixelHeight;
            float topPadding = isLandscape ? LandscapeTopPadding : config.BoardTopHudPadding;
            float bottomPadding = isLandscape ? LandscapeBottomPadding : config.BoardBottomPadding;
            float top = cameraPosition.y + halfHeight - topPadding;
            float bottom = cameraPosition.y - halfHeight + bottomPadding;
            float availableWidth = Mathf.Max(0.1f, right - left);
            float availableHeight = Mathf.Max(0.1f, top - bottom);

            result = isLandscape
                ? CalculateLandscape(config, left, right, top, bottom, availableWidth, availableHeight)
                : CalculatePortrait(config, cameraPosition, top, bottom, availableWidth, availableHeight);

            return true;
        }

        private static SolitaireBoardLayoutResult CalculatePortrait(
            SolitaireDeckConfigSO config,
            Vector3 cameraPosition,
            float top,
            float bottom,
            float availableWidth,
            float availableHeight)
        {
            float spacingRatio = config.CardHorizontalSpacingRatio;
            int tableauCount = SolitaireCardUtility.TableauCount;
            float maxCardWidthForViewport = availableWidth / (tableauCount + ((tableauCount - 1) * spacingRatio));
            float cardWidth = Mathf.Min(maxCardWidthForViewport, config.MaxResponsiveCardWidth);
            float cardHeight = cardWidth * config.CardAspectRatio;
            float rowGap = config.RowVerticalGap;
            float maxCardHeightFromVertical = (availableHeight - (2f * rowGap)) / (3f + PortraitMinTableauStackHeightFactor);

            if (maxCardHeightFromVertical > 0f && cardHeight > maxCardHeightFromVertical)
            {
                cardHeight = maxCardHeightFromVertical;
                cardWidth = Mathf.Min(cardHeight / config.CardAspectRatio, maxCardWidthForViewport);
                cardHeight = cardWidth * config.CardAspectRatio;
            }

            float gap = cardWidth * spacingRatio;
            float stockWasteRowY = top - (cardHeight * 0.5f);
            float foundationRowY = stockWasteRowY - cardHeight - rowGap;
            float tableauY = foundationRowY - cardHeight - rowGap;
            float stockWasteStartX = GetCenteredRowStartX(cameraPosition.x, 2, cardWidth, gap);
            float foundationStartX = GetCenteredRowStartX(cameraPosition.x, SolitaireCardUtility.FoundationCount, cardWidth, gap);
            float tableauStartX = GetCenteredRowStartX(cameraPosition.x, SolitaireCardUtility.TableauCount, cardWidth, gap);
            float cardScale = cardWidth / Mathf.Max(0.01f, config.CardSize.x);

            var foundationPositions = new Vector3[SolitaireCardUtility.FoundationCount];
            var tableauPositions = new Vector3[SolitaireCardUtility.TableauCount];

            for (int i = 0; i < foundationPositions.Length; i++)
            {
                float x = foundationStartX + (i * (cardWidth + gap));
                foundationPositions[i] = new Vector3(x, foundationRowY, 0f);
            }

            for (int i = 0; i < tableauPositions.Length; i++)
            {
                float x = tableauStartX + (i * (cardWidth + gap));
                tableauPositions[i] = new Vector3(x, tableauY, 0f);
            }

            return new SolitaireBoardLayoutResult
            {
                CardSize = new Vector2(cardWidth, cardHeight),
                TableauBottomPlayableY = bottom + (cardHeight * 0.5f),
                WasteStackXOffset = config.WasteStackXOffset * cardScale,
                FaceDownTableauYOffset = config.FaceDownTableauYOffset * cardScale,
                FaceUpTableauYOffset = config.FaceUpTableauYOffset * cardScale,
                MinCompressedFaceUpYOffset = config.MinCompressedFaceUpYOffset * cardScale,
                CardScale = cardScale,
                StockPosition = new Vector3(stockWasteStartX, stockWasteRowY, 0f),
                WastePosition = new Vector3(stockWasteStartX + cardWidth + gap, stockWasteRowY, 0f),
                FoundationPositions = foundationPositions,
                TableauPositions = tableauPositions,
            };
        }

        private static SolitaireBoardLayoutResult CalculateLandscape(
            SolitaireDeckConfigSO config,
            float left,
            float right,
            float top,
            float bottom,
            float availableWidth,
            float availableHeight)
        {
            float spacingRatio = config.CardHorizontalSpacingRatio;
            float rowGap = config.RowVerticalGap;
            float maxCardWidthForTableau = availableWidth /
                                           (SolitaireCardUtility.TableauCount + ((SolitaireCardUtility.TableauCount - 1) * spacingRatio));
            float maxCardWidthForTopRow = availableWidth /
                                          (6f + (4f * spacingRatio) + LandscapeTopGroupGapRatio);
            float maxCardWidthForViewport = Mathf.Min(maxCardWidthForTableau, maxCardWidthForTopRow);
            float cardWidth = Mathf.Min(maxCardWidthForViewport, LandscapeMaxCardWidth);
            float cardHeight = cardWidth * config.CardAspectRatio;
            float maxCardHeightFromVertical = (availableHeight - rowGap) / (2f + LandscapeMinTableauStackHeightFactor);

            if (maxCardHeightFromVertical > 0f && cardHeight > maxCardHeightFromVertical)
            {
                cardHeight = maxCardHeightFromVertical;
                cardWidth = Mathf.Min(cardHeight / config.CardAspectRatio, maxCardWidthForViewport, LandscapeMaxCardWidth);
                cardHeight = cardWidth * config.CardAspectRatio;
            }

            float compactGap = cardWidth * spacingRatio;
            float tableauGap = GetDistributedGap(availableWidth, SolitaireCardUtility.TableauCount, cardWidth, compactGap);
            float foundationGap = Mathf.Max(compactGap, Mathf.Min(tableauGap, cardWidth * 0.45f));
            float topRowY = top - (cardHeight * 0.5f);
            float naturalTableauY = topRowY - cardHeight - rowGap;
            float lowerTableauY = bottom + (cardHeight * 0.75f);
            float tableauY = Mathf.Lerp(naturalTableauY, lowerTableauY, 0.70f);
            float cardScale = cardWidth / Mathf.Max(0.01f, config.CardSize.x);
            var foundationPositions = new Vector3[SolitaireCardUtility.FoundationCount];
            var tableauPositions = new Vector3[SolitaireCardUtility.TableauCount];

            for (int i = 0; i < tableauPositions.Length; i++)
            {
                float x = left + (cardWidth * 0.5f) + (i * (cardWidth + tableauGap));
                tableauPositions[i] = new Vector3(x, tableauY, 0f);
            }

            float stockX = left + (cardWidth * 0.5f);
            float wasteX = stockX + cardWidth + compactGap;
            float foundationRowWidth = (SolitaireCardUtility.FoundationCount * cardWidth) +
                                       ((SolitaireCardUtility.FoundationCount - 1) * foundationGap);
            float foundationStartX = right - foundationRowWidth + (cardWidth * 0.5f);
            float minimumGroupGap = Mathf.Max(compactGap, cardWidth * LandscapeTopGroupGapRatio);
            bool topGroupsOverlap = wasteX + (cardWidth * 0.5f) + minimumGroupGap >
                                    foundationStartX - (cardWidth * 0.5f);

            if (topGroupsOverlap)
            {
                float topGap = GetDistributedGap(availableWidth, 6, cardWidth, compactGap);
                stockX = GetRowStartX(left, cardWidth);
                wasteX = stockX + cardWidth + topGap;
                foundationStartX = wasteX + cardWidth + topGap;
                foundationGap = topGap;
            }

            for (int i = 0; i < foundationPositions.Length; i++)
            {
                float x = foundationStartX + (i * (cardWidth + foundationGap));
                foundationPositions[i] = new Vector3(x, topRowY, 0f);
            }

            return new SolitaireBoardLayoutResult
            {
                CardSize = new Vector2(cardWidth, cardHeight),
                TableauBottomPlayableY = bottom + (cardHeight * 0.5f),
                WasteStackXOffset = config.WasteStackXOffset * cardScale,
                FaceDownTableauYOffset = config.FaceDownTableauYOffset * cardScale,
                FaceUpTableauYOffset = config.FaceUpTableauYOffset * cardScale,
                MinCompressedFaceUpYOffset = config.MinCompressedFaceUpYOffset * cardScale,
                CardScale = cardScale,
                StockPosition = new Vector3(stockX, topRowY, 0f),
                WastePosition = new Vector3(wasteX, topRowY, 0f),
                FoundationPositions = foundationPositions,
                TableauPositions = tableauPositions,
            };
        }

        private static float GetDistributedGap(float availableWidth, int columnCount, float cardWidth, float minimumGap)
        {
            if (columnCount <= 1)
                return minimumGap;

            float distributedGap = (availableWidth - (columnCount * cardWidth)) / (columnCount - 1);
            return Mathf.Max(minimumGap, distributedGap);
        }

        private static float GetRowStartX(float left, float cardWidth)
        {
            return left + (cardWidth * 0.5f);
        }

        public static void GetLayoutFrustum(Camera camera, out float halfWidth, out float halfHeight)
        {
            halfHeight = camera.orthographicSize;
            int pixelWidth = Mathf.Max(1, camera.pixelWidth);
            int pixelHeight = Mathf.Max(1, camera.pixelHeight);
            float aspect = (float)pixelWidth / pixelHeight;
            halfWidth = halfHeight * aspect;
        }

        public static float GetCenteredRowStartX(float centerX, int columnCount, float cardWidth, float gap)
        {
            float rowWidth = (columnCount * cardWidth) + ((columnCount - 1) * gap);
            return centerX - (rowWidth * 0.5f) + (cardWidth * 0.5f);
        }
    }
}
