using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed partial class CardView
    {
        private void EnsureDragShadow()
        {
            dragShadowRenderer = CardViewLogic.DragShadow.Resolve(
                dragShadowRenderer,
                CardViewLogic.ChildRenderer.Find(transform, CardViewLogic.Constants.DragShadowChildName),
                _isDragVisualActive,
                CreateDragShadowRenderer);
            ResetDragShadow();
        }

        private SpriteRenderer CreateDragShadowRenderer()
        {
            var shadowObject = new GameObject(CardViewLogic.Constants.DragShadowChildName);
            shadowObject.transform.SetParent(transform, false);
            shadowObject.transform.localPosition = CardViewLogic.DragVisual.ResolveDefaultShadowLocalPosition();

            var renderer = shadowObject.AddComponent<SpriteRenderer>();
            CardViewLogic.DragShadow.ApplyCopyFromCard(renderer, new CardViewLogic.DragShadow.CopyFromCardState(cardRenderer));
            return renderer;
        }

        private void ResetDragShadow() => CardViewLogic.DragShadow.ApplyReset(dragShadowRenderer);

        private void EnsureSelectionHighlight()
        {
            selectionHighlightRenderer = CardViewLogic.SelectionHighlight.Resolve(
                selectionHighlightRenderer,
                CardViewLogic.ChildRenderer.Find(transform, CardViewLogic.Constants.SelectionHighlightChildName),
                _isSelectionHighlightActive,
                CreateSelectionHighlightRenderer);
        }

        private SpriteRenderer CreateSelectionHighlightRenderer()
        {
            var highlightObject = new GameObject(CardViewLogic.Constants.SelectionHighlightChildName);
            highlightObject.transform.SetParent(transform, false);
            highlightObject.transform.localPosition = CardViewLogic.SelectionHighlight.ResolveLocalPosition();

            var renderer = highlightObject.AddComponent<SpriteRenderer>();
            renderer.enabled = false;
            CardViewLogic.SelectionHighlight.ApplyConfigureFromCard(renderer, cardRenderer);
            return renderer;
        }

        private void ApplySelectionHighlightSize(Vector2 worldSize, float cardScale)
        {
            if (!CardViewLogic.SelectionHighlight.CanApplySize(selectionHighlightRenderer))
                return;

            if (!CardViewLogic.Layout.TryResolveHighlightScale(
                    worldSize,
                    selectionHighlightRenderer.sprite.bounds.size,
                    cardScale,
                    out Vector3 localScale))
                return;

            CardViewLogic.SpriteRendererOps.SetLocalScale(selectionHighlightRenderer.transform, localScale);
        }
    }
}
