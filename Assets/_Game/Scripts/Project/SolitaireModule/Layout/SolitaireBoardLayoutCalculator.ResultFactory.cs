using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal static class ResultFactory
        {
            public static SolitaireBoardLayoutResult FromConfig(SolitaireDeckConfigSO config) =>
                new SolitaireBoardLayoutResult
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

            public static SolitaireBoardLayoutResult Create(
                SolitaireDeckConfigSO config,
                Vector2 cardSize,
                float cardScale,
                float bottom,
                float stockX,
                float stockWasteRowY,
                float cardWidth,
                float gap,
                Vector3[] foundationPositions,
                Vector3[] tableauPositions,
                float? wasteXOverride = null)
            {
                float wasteX = wasteXOverride ?? stockX + cardWidth + gap;

                return new SolitaireBoardLayoutResult
                {
                    CardSize = cardSize,
                    TableauBottomPlayableY = ScaledOffsets.GetTableauBottomPlayableY(bottom, cardSize.y),
                    WasteStackXOffset = ScaledOffsets.Scale(config.WasteStackXOffset, cardScale),
                    FaceDownTableauYOffset = ScaledOffsets.Scale(config.FaceDownTableauYOffset, cardScale),
                    FaceUpTableauYOffset = ScaledOffsets.Scale(config.FaceUpTableauYOffset, cardScale),
                    MinCompressedFaceUpYOffset = ScaledOffsets.Scale(config.MinCompressedFaceUpYOffset, cardScale),
                    CardScale = cardScale,
                    StockPosition = new Vector3(stockX, stockWasteRowY, 0f),
                    WastePosition = new Vector3(wasteX, stockWasteRowY, 0f),
                    FoundationPositions = foundationPositions,
                    TableauPositions = tableauPositions,
                };
            }
        }
    }
}
