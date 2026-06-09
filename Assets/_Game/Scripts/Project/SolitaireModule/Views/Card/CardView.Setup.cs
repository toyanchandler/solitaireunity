using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed partial class CardView
    {
        private void ResolveRequiredComponents()
        {
            motionPresenter ??= GetComponent<CardMotionPresenter>();
            CardViewLogic.Components.RequireMotionPresenter(motionPresenter, name);
            _dragBehaviour = GetComponent<CardDragBehaviour>();
            CardViewLogic.Components.RequireDragBehaviour(_dragBehaviour, name);
        }

        private void CacheHomeTransformState()
        {
            _homeScale = CardViewLogic.HomeState.ReadScale(transform);
            _homeColor = CardViewLogic.HomeState.ReadCardColor(cardRenderer);
        }

        private void SyncIdentity(CardState state)
        {
            if (!CardViewLogic.Identity.ShouldSync(identity, state.Id))
                return;

            identity.SetIdentity(state.Id);
        }

        private void ApplyResolvedLayoutScale(Vector2 worldSize, float scale)
        {
            CardViewLogic.Layout.ResolvedScale resolved =
                CardViewLogic.Layout.CreateResolvedScale(worldSize, scale, transform.localScale.z);

            _homeScale = resolved.HomeScale;
            transform.localScale = _homeScale;
            motionPresenter.SetHomeScale(_homeScale);
            ApplyColliderSize(resolved.ColliderSize);
            ApplySelectionHighlightSize(worldSize, scale);
        }

        private void ApplyColliderSize(Vector2 colliderSize)
        {
            TryGetComponent(out BoxCollider2D cardCollider);
            CardViewLogic.Collider.ApplySize(cardCollider, colliderSize);
        }

        private void DisableSortingGroup()
        {
            if (!CardViewLogic.SortingGroupState.CanDisable(sortingGroup))
                return;

            sortingGroup.enabled = false;
        }

        private void ApplySortingOrders(CardViewLogic.Sorting.ApplyValues values)
        {
            CardViewLogic.SpriteRendererOps.SetSortingOrder(cardRenderer, values.CardOrder);
            CardViewLogic.SpriteRendererOps.SetSortingOrder(dragShadowRenderer, values.DragShadowOrder);
            CardViewLogic.SpriteRendererOps.SetSortingOrder(selectionHighlightRenderer, values.SelectionHighlightOrder);
        }
    }
}
