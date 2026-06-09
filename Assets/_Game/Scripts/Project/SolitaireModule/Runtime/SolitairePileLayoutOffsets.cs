using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal delegate Vector3 SolitairePileOffsetCalculator(
        SolitaireBoardState board,
        SolitaireRuntimeLayoutMetrics metrics,
        SolitaireDeckConfigSO config,
        PileRef pileRef,
        Vector3 basePosition,
        int index,
        CardState card);

    internal static class SolitairePileLayoutOffsets
    {
        private static readonly SolitairePileOffsetCalculator[] Calculators = CreateCalculators();

        public static Vector3 Calculate(
            SolitaireBoardState board,
            SolitaireRuntimeLayoutMetrics metrics,
            SolitaireDeckConfigSO config,
            PileRef pileRef,
            Vector3 basePosition,
            int index,
            CardState card)
        {
            SolitairePileOffsetCalculator calculator = Calculators[(int)pileRef.Type];
            Vector3 offset = calculator(board, metrics, config, pileRef, basePosition, index, card);
            return basePosition + offset;
        }

        private static SolitairePileOffsetCalculator[] CreateCalculators()
        {
            var calculators = new SolitairePileOffsetCalculator[4];
            calculators[(int)SolitairePileType.Tableau] = CalculateTableauOffset;
            calculators[(int)SolitairePileType.Waste] = CalculateWasteOffset;
            calculators[(int)SolitairePileType.Stock] = CalculateStockOffset;
            calculators[(int)SolitairePileType.Foundation] = CalculateFoundationOffset;
            return calculators;
        }

        private static Vector3 CalculateTableauOffset(
            SolitaireBoardState board,
            SolitaireRuntimeLayoutMetrics metrics,
            SolitaireDeckConfigSO config,
            PileRef pileRef,
            Vector3 basePosition,
            int index,
            CardState card)
        {
            float y = 0f;
            FixedCardPileState pile = board.GetPile(pileRef);
            float faceUpOffset = CalculateTableauFaceUpOffset(board, pile, basePosition.y, metrics);

            for (int i = 0; i < index; i++)
            {
                CardState previous = board.GetCard(pile[i]);
                y -= previous.IsFaceUp ? faceUpOffset : metrics.FaceDownTableauYOffset;
            }

            return new Vector3(0f, y, index * config.CardZStep);
        }

        private static Vector3 CalculateWasteOffset(
            SolitaireBoardState board,
            SolitaireRuntimeLayoutMetrics metrics,
            SolitaireDeckConfigSO config,
            PileRef pileRef,
            Vector3 basePosition,
            int index,
            CardState card)
        {
            FixedCardPileState wastePile = board.GetPile(pileRef);
            const int maxVisibleWasteCards = 3;
            int firstVisibleIndex = Mathf.Max(0, wastePile.Count - maxVisibleWasteCards);
            int visibleOffset = Mathf.Clamp(index - firstVisibleIndex, 0, maxVisibleWasteCards - 1);
            return new Vector3(visibleOffset * metrics.WasteStackXOffset, 0f, index * config.CardZStep);
        }

        private static Vector3 CalculateStockOffset(
            SolitaireBoardState board,
            SolitaireRuntimeLayoutMetrics metrics,
            SolitaireDeckConfigSO config,
            PileRef pileRef,
            Vector3 basePosition,
            int index,
            CardState card)
        {
            return new Vector3(0f, 0f, index * config.StockZStep);
        }

        private static Vector3 CalculateFoundationOffset(
            SolitaireBoardState board,
            SolitaireRuntimeLayoutMetrics metrics,
            SolitaireDeckConfigSO config,
            PileRef pileRef,
            Vector3 basePosition,
            int index,
            CardState card)
        {
            return new Vector3(0f, 0f, index * config.CardZStep);
        }

        private static float CalculateTableauFaceUpOffset(
            SolitaireBoardState board,
            FixedCardPileState pile,
            float slotTopY,
            SolitaireRuntimeLayoutMetrics metrics)
        {
            if (pile.Count <= 1)
                return metrics.FaceUpTableauYOffset;

            float hiddenHeight = 0f;
            int faceUpOffsetCount = 0;

            for (int i = 0; i < pile.Count - 1; i++)
            {
                CardState pileCard = board.GetCard(pile[i]);
                faceUpOffsetCount += pileCard.IsFaceUp ? 1 : 0;
                hiddenHeight += pileCard.IsFaceUp ? 0f : metrics.FaceDownTableauYOffset;
            }

            if (faceUpOffsetCount == 0)
                return metrics.FaceUpTableauYOffset;

            float availableStackHeight = Mathf.Abs(slotTopY - metrics.TableauBottomPlayableY) - metrics.CardSize.y;
            float requiredHeight = hiddenHeight + faceUpOffsetCount * metrics.FaceUpTableauYOffset;

            if (requiredHeight <= availableStackHeight)
                return metrics.FaceUpTableauYOffset;

            float compressed = (availableStackHeight - hiddenHeight) / faceUpOffsetCount;
            return Mathf.Clamp(compressed, metrics.MinCompressedFaceUpYOffset, metrics.FaceUpTableauYOffset);
        }
    }
}
