using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static partial class CardViewLogic
    {
        internal static class Sprites
        {
            public static Sprite ResolveFaceSprite(SolitaireDeckConfigSO config, CardState state, bool isFaceUp) =>
                isFaceUp ? config.GetCardFrontSprite(state) : config.GetCardBackSprite(state);

            public static bool ShouldSyncDragShadowSprite(Sprite shadowSprite, bool isDragVisualActive) =>
                shadowSprite == null && isDragVisualActive;
        }

        internal static class DragVisual
        {
            public static Vector3 ResolveDragScale(Vector3 homeScale, float dragLiftScale) =>
                homeScale * dragLiftScale;

            public static Color ApplyDragAlpha(Color homeColor, float dragAlpha)
            {
                Color color = homeColor;
                color.a = dragAlpha;
                return color;
            }

            public static Vector3 ResolveShadowLocalPosition(Vector2 offset) =>
                new Vector3(offset.x, offset.y, Constants.DragShadowDepthZ);

            public static Vector3 ResolveDefaultShadowLocalPosition() =>
                new Vector3(
                    Constants.DefaultDragShadowOffsetX,
                    Constants.DefaultDragShadowOffsetY,
                    Constants.DragShadowDepthZ);

            public static Color BuildShadowColor(float shadowAlpha)
            {
                Color shadowColor = Color.black;
                shadowColor.a = shadowAlpha;
                return shadowColor;
            }

            public static Color ClearShadowAlpha(Color currentColor)
            {
                Color shadowColor = currentColor;
                shadowColor.a = 0f;
                return shadowColor;
            }

            public readonly struct ShowState
            {
                public ShowState(SolitaireDeckConfigSO config)
                {
                    Enabled = true;
                    LocalPosition = ResolveShadowLocalPosition(config.DragShadowOffset);
                    Color = BuildShadowColor(config.DragShadowAlpha);
                }

                public bool Enabled { get; }
                public Vector3 LocalPosition { get; }
                public Color Color { get; }
            }
        }

        internal static class DragShadow
        {
            public static bool IsAssigned(SpriteRenderer renderer) => renderer != null;

            public static bool NeedsSpriteSync(
                SpriteRenderer shadowRenderer,
                bool isDragVisualActive) =>
                IsAssigned(shadowRenderer) &&
                Sprites.ShouldSyncDragShadowSprite(shadowRenderer.sprite, isDragVisualActive);

            public static bool CanCopySpriteFromCard(SpriteRenderer shadowRenderer, SpriteRenderer cardRenderer) =>
                IsAssigned(shadowRenderer) &&
                shadowRenderer.sprite == null &&
                MainRenderer.IsAssigned(cardRenderer);

            public static SpriteRenderer Activate(SpriteRenderer renderer, bool isDragVisualActive)
            {
                renderer.enabled = isDragVisualActive;
                return renderer;
            }

            public static SpriteRenderer Resolve(
                SpriteRenderer assigned,
                SpriteRenderer found,
                bool isDragVisualActive,
                Func<SpriteRenderer> createRenderer) =>
                ChildRenderer.SelectSource(assigned, found) switch
                {
                    ChildRenderer.ResolveSource.Assigned => Activate(assigned, isDragVisualActive),
                    ChildRenderer.ResolveSource.Found => Activate(found, isDragVisualActive),
                    _ => createRenderer()
                };

            public readonly struct CopyFromCardState
            {
                public CopyFromCardState(SpriteRenderer cardRenderer)
                {
                    Sprite = cardRenderer.sprite;
                    SortingLayerId = cardRenderer.sortingLayerID;
                    SortingOrder = Sorting.ResolveDragShadowOrder(cardRenderer.sortingOrder);
                }

                public Sprite Sprite { get; }
                public int SortingLayerId { get; }
                public int SortingOrder { get; }
            }

            public static void ApplyCopyFromCard(SpriteRenderer shadowRenderer, CopyFromCardState state)
            {
                if (!IsAssigned(shadowRenderer))
                    return;

                shadowRenderer.sprite = state.Sprite;
                shadowRenderer.sortingLayerID = state.SortingLayerId;
                shadowRenderer.sortingOrder = state.SortingOrder;
            }

            public static void ApplyShowState(SpriteRenderer shadowRenderer, DragVisual.ShowState showState)
            {
                if (!IsAssigned(shadowRenderer))
                    return;

                shadowRenderer.enabled = showState.Enabled;
                SpriteRendererOps.SetLocalPosition(shadowRenderer.transform, showState.LocalPosition);
                shadowRenderer.color = showState.Color;
            }

            public static void ApplyReset(SpriteRenderer shadowRenderer)
            {
                if (!IsAssigned(shadowRenderer))
                    return;

                shadowRenderer.enabled = false;
                shadowRenderer.color = DragVisual.ClearShadowAlpha(shadowRenderer.color);
            }
        }
    }
}
