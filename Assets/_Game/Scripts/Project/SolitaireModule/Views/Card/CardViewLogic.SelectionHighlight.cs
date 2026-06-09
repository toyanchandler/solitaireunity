using System;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static partial class CardViewLogic
    {
        internal static class SelectionHighlight
        {
            public static Vector3 ResolveLocalPosition() =>
                new Vector3(0f, 0f, Constants.SelectionHighlightDepthZ);

            public static bool IsAssigned(SpriteRenderer renderer) => renderer != null;

            public static bool CanApplySize(SpriteRenderer renderer) =>
                IsAssigned(renderer) && renderer.sprite != null;

            public static void ApplyActiveState(SpriteRenderer renderer, bool isActive)
            {
                if (!IsAssigned(renderer))
                    return;

                renderer.enabled = isActive;

                if (!isActive)
                    return;

                renderer.color = Constants.SelectionHighlightColor;
            }

            public static SpriteRenderer Resolve(
                SpriteRenderer assigned,
                SpriteRenderer found,
                bool isHighlightActive,
                Func<SpriteRenderer> createRenderer) =>
                ChildRenderer.SelectSource(assigned, found) switch
                {
                    ChildRenderer.ResolveSource.Assigned => assigned,
                    ChildRenderer.ResolveSource.Found => ActivateFound(found, isHighlightActive),
                    _ => createRenderer()
                };

            public static SpriteRenderer ActivateFound(SpriteRenderer found, bool isHighlightActive)
            {
                found.enabled = isHighlightActive;
                return found;
            }

            public readonly struct ConfigureFromCardState
            {
                public ConfigureFromCardState(SpriteRenderer cardRenderer)
                {
                    SortingLayerId = cardRenderer.sortingLayerID;
                    SortingOrder = Sorting.ResolveSelectionHighlightOrder(cardRenderer.sortingOrder);
                }

                public int SortingLayerId { get; }
                public int SortingOrder { get; }
            }

            public static void ApplyConfigureFromCard(
                SpriteRenderer highlightRenderer,
                SpriteRenderer cardRenderer)
            {
                if (!IsAssigned(highlightRenderer) || !MainRenderer.IsAssigned(cardRenderer))
                    return;

                ConfigureFromCardState state = new ConfigureFromCardState(cardRenderer);
                highlightRenderer.sortingLayerID = state.SortingLayerId;
                highlightRenderer.sortingOrder = state.SortingOrder;
                highlightRenderer.sprite ??= Resources.Load<Sprite>(Constants.SelectionHighlightSpriteResourcePath);
            }
        }
    }
}
