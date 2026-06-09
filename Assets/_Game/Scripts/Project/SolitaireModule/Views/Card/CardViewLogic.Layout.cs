using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static partial class CardViewLogic
    {
        internal static class Layout
        {
            public static bool HasRenderableSprite(SpriteRenderer renderer) =>
                renderer != null && renderer.sprite != null;

            public static bool TryResolveUniformScale(Vector2 worldSize, Vector2 spriteSize, out float scale)
            {
                scale = 0f;

                if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                    return false;

                scale = Mathf.Min(worldSize.x / spriteSize.x, worldSize.y / spriteSize.y);
                return scale > 0f;
            }

            public static Vector3 BuildHomeScale(float scale, float currentScaleZ) =>
                new Vector3(scale, scale, currentScaleZ);

            public static Vector2 ResolveColliderSize(Vector2 worldSize, float scale) =>
                new Vector2(worldSize.x / scale, worldSize.y / scale);

            public static bool TryResolveHighlightScale(
                Vector2 worldSize,
                Vector2 spriteSize,
                float cardScale,
                out Vector3 localScale)
            {
                localScale = Vector3.one;

                if (spriteSize.x <= 0f || spriteSize.y <= 0f || cardScale <= 0f)
                    return false;

                float highlightScale = Mathf.Min(
                    worldSize.x / (spriteSize.x * cardScale),
                    worldSize.y / (spriteSize.y * cardScale));
                localScale = new Vector3(highlightScale, highlightScale, Constants.HighlightLocalScaleZ);
                return true;
            }

            public readonly struct ResolvedScale
            {
                public ResolvedScale(Vector2 worldSize, float scale, float currentScaleZ)
                {
                    WorldSize = worldSize;
                    Scale = scale;
                    HomeScale = BuildHomeScale(scale, currentScaleZ);
                    ColliderSize = ResolveColliderSize(worldSize, scale);
                }

                public Vector2 WorldSize { get; }
                public float Scale { get; }
                public Vector3 HomeScale { get; }
                public Vector2 ColliderSize { get; }
            }

            public static ResolvedScale CreateResolvedScale(Vector2 worldSize, float scale, float currentScaleZ) =>
                new ResolvedScale(worldSize, scale, currentScaleZ);
        }
    }
}
