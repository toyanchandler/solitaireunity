using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static partial class CardViewLogic
    {
        internal static class Sorting
        {
            public static int ResolveDragShadowOrder(int cardOrder) =>
                cardOrder + Constants.DragShadowSortingOffset;

            public static int ResolveSelectionHighlightOrder(int cardOrder) =>
                cardOrder + Constants.SelectionHighlightSortingOffset;

            public readonly struct ApplyValues
            {
                public ApplyValues(int cardOrder)
                {
                    CardOrder = cardOrder;
                    DragShadowOrder = ResolveDragShadowOrder(cardOrder);
                    SelectionHighlightOrder = ResolveSelectionHighlightOrder(cardOrder);
                }

                public int CardOrder { get; }
                public int DragShadowOrder { get; }
                public int SelectionHighlightOrder { get; }
            }
        }

        internal static class SortingGroupState
        {
            public static bool CanDisable(SortingGroup sortingGroup) => sortingGroup != null;
        }

        internal static class MainRenderer
        {
            public static bool IsAssigned(SpriteRenderer renderer) => renderer != null;

            public static bool IsVisible(SpriteRenderer renderer) =>
                renderer != null && renderer.enabled;
        }

        internal static class SpriteRendererOps
        {
            public static void SetSortingOrder(SpriteRenderer renderer, int order)
            {
                if (!MainRenderer.IsAssigned(renderer))
                    return;

                renderer.sortingOrder = order;
            }

            public static void SetEnabled(SpriteRenderer renderer, bool isEnabled)
            {
                if (!MainRenderer.IsAssigned(renderer))
                    return;

                renderer.enabled = isEnabled;
            }

            public static void SetColor(SpriteRenderer renderer, Color color)
            {
                if (!MainRenderer.IsAssigned(renderer))
                    return;

                renderer.color = color;
            }

            public static void SetSprite(SpriteRenderer renderer, Sprite sprite)
            {
                if (!MainRenderer.IsAssigned(renderer))
                    return;

                renderer.sprite = sprite;
            }

            public static void SetLocalScale(Transform transform, Vector3 localScale)
            {
                if (transform == null)
                    return;

                transform.localScale = localScale;
            }

            public static void SetLocalPosition(Transform transform, Vector3 localPosition)
            {
                if (transform == null)
                    return;

                transform.localPosition = localPosition;
            }
        }

        internal static class Collider
        {
            public static bool IsPresent(BoxCollider2D collider) => collider != null;

            public static void ApplySize(BoxCollider2D collider, Vector2 size)
            {
                if (!IsPresent(collider))
                    return;

                collider.size = size;
            }
        }

        internal static class ChildRenderer
        {
            public enum ResolveSource
            {
                Assigned,
                Found,
                Create
            }

            public static ResolveSource SelectSource(SpriteRenderer assigned, SpriteRenderer found) =>
                assigned != null ? ResolveSource.Assigned :
                found != null ? ResolveSource.Found :
                ResolveSource.Create;

            public static SpriteRenderer Find(Transform parent, string childName)
            {
                Transform child = parent.Find(childName);
                return child != null ? child.GetComponent<SpriteRenderer>() : null;
            }
        }
    }
}
