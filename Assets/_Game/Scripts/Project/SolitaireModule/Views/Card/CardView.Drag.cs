using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed partial class CardView
    {
        public void BeginDrag(Transform dragParent, Vector3 pointerWorld, SolitaireDeckConfigSO config, int sortingOrder)
        {
            _dragBehaviour.BeginDrag(dragParent, pointerWorld);
            SetSortingOrder(sortingOrder);
            BeginDragVisual(config);
        }

        public void MoveDrag(Vector3 pointerWorld) => _dragBehaviour.MoveToPointer(pointerWorld);

        public void FinishDrag()
        {
            _dragBehaviour.EndDrag(CachedTransform.parent);
            EndDragVisual();
        }

        public void BeginDragVisual(SolitaireDeckConfigSO config)
        {
            if (CardViewLogic.Guard.ShouldSkipDragVisual(config, IsPresenting))
                return;

            ActivateDragVisualState();
            transform.localScale = CardViewLogic.DragVisual.ResolveDragScale(_homeScale, config.DragLiftScale);
            CardViewLogic.SpriteRendererOps.SetColor(
                cardRenderer,
                CardViewLogic.DragVisual.ApplyDragAlpha(_homeColor, config.DragAlpha));
            ShowDragShadow(config);
        }

        public void EndDragVisual()
        {
            _isDragVisualActive = false;
            SetSelectionHighlight(false);
            ResetFeedback();
            ResetDragShadow();
        }

        private void ActivateDragVisualState()
        {
            SetCardRendererVisible(true);
            SetSelectionHighlight(true);
            _isDragVisualActive = true;
            visualStateMachine?.SetState(CardVisualState.Dragging);
        }

        private void ShowDragShadow(SolitaireDeckConfigSO config)
        {
            EnsureDragShadowSprite();
            CardViewLogic.DragShadow.ApplyShowState(dragShadowRenderer, new CardViewLogic.DragVisual.ShowState(config));
        }

        private void EnsureDragShadowSprite()
        {
            if (!CardViewLogic.DragShadow.CanCopySpriteFromCard(dragShadowRenderer, cardRenderer))
                return;

            dragShadowRenderer.sprite = cardRenderer.sprite;
        }

        private void SyncDragShadowSprite()
        {
            if (!CardViewLogic.DragShadow.NeedsSpriteSync(dragShadowRenderer, _isDragVisualActive))
                return;

            dragShadowRenderer.sprite = cardRenderer.sprite;
        }
    }
}
