using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public static class SolitaireCardSelectionVisuals
    {
        public static void SetSelectionHighlight(SolitaireRuntimeContext context, int cardId, bool isHighlighted)
        {
            if (context == null || cardId < 0)
                return;

            CardView card = context.ViewRegistry.GetCard(cardId);
            card?.SetSelectionHighlight(isHighlighted);
        }

        public static void ClearSelectionHighlight(SolitaireRuntimeContext context, int cardId)
        {
            SetSelectionHighlight(context, cardId, false);
        }

        public static void ClearAll(SolitaireRuntimeContext context)
        {
            if (context?.ViewRegistry?.Cards == null)
                return;

            CardView[] cards = context.ViewRegistry.Cards;

            for (int i = 0; i < cards.Length; i++)
                cards[i]?.SetSelectionHighlight(false);
        }

        public static void ApplyDragHighlights(SolitaireRuntimeContext context, int[] cardIds, int count, bool isHighlighted)
        {
            if (context == null || cardIds == null)
                return;

            for (int i = 0; i < count; i++)
                SetSelectionHighlight(context, cardIds[i], isHighlighted);
        }
    }
}
