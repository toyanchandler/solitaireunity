using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static class SolitaireSlotAnchorLogic
    {
        internal static class Constants
        {
            public const float HalfMultiplier = 0.5f;
            public const float OpaqueAlpha = 1f;
            public const float ZeroOffset = 0f;
            public const float MinPositiveSize = 0f;
            public const float PulseStartScale = 0.82f;
            public const float PulseStartAlpha = 0.95f;
            public const float PulseEasingPower = 2f;
            public const float PulseFadeCompletion = 1f;
            public const string PulseSpriteResourcePath = "Solitaire/card_selection_highlight";
            public const string MissingColliderMessageSuffix = " is missing BoxCollider2D.";

            public static readonly Color DefaultSlot = Color.white;
            public static readonly Color FoundationPulse = new Color(0.35f, 0.72f, 1f, 0.9f);
            public static readonly Vector3 UnitLocalScale = Vector3.one;
        }

        internal static class Colors
        {
            public static Color ResolveHighlightColor(bool isHighlighted, Color activeColor) =>
                isHighlighted ? activeColor : Constants.DefaultSlot;

            public static Color WithOpaqueAlpha(Color color) =>
                new Color(color.r, color.g, color.b, Constants.OpaqueAlpha);

            public static Color WithFadingAlpha(Color startColor, float normalizedTime) =>
                new Color(
                    startColor.r,
                    startColor.g,
                    startColor.b,
                    startColor.a * (Constants.PulseFadeCompletion - normalizedTime));

            public static Color CreatePulseStartColor() =>
                new Color(
                    Constants.FoundationPulse.r,
                    Constants.FoundationPulse.g,
                    Constants.FoundationPulse.b,
                    Constants.PulseStartAlpha);
        }

        internal static class Highlight
        {
            public static bool CanApply(SpriteRenderer renderer) => renderer != null;

            public static bool ShouldEnableRenderer(bool isHighlighted, bool isSlotVisualVisible) =>
                isHighlighted || isSlotVisualVisible;
        }

        internal static class Layout
        {
            public static bool CanApplyCollider(BoxCollider2D collider) => collider != null;

            public static bool CanApplyHighlightLayout(SpriteRenderer renderer) => renderer != null;

            internal static class TableauDropArea
            {
                public static float ComputeColumnTopY(float slotY, float cardHeight) =>
                    slotY + (cardHeight * Constants.HalfMultiplier);

                public static float ComputeColumnHeight(float cardHeight, float columnTopY, float columnBottomY) =>
                    Mathf.Max(cardHeight, columnTopY - columnBottomY);

                public static Vector2 ComputeSize(float cardWidth, float columnHeight) =>
                    new Vector2(cardWidth, columnHeight);

                public static float ComputeVerticalOffset(float columnHeight, float cardHeight) =>
                    -(columnHeight - cardHeight) * Constants.HalfMultiplier;

                public static Vector2 ComputeOffset(float columnHeight, float cardHeight) =>
                    new Vector2(Constants.ZeroOffset, ComputeVerticalOffset(columnHeight, cardHeight));

                public static (Vector2 size, Vector2 offset) Compute(
                    Vector3 slotPosition,
                    Vector2 cardSize,
                    float columnBottomY)
                {
                    float columnTopY = ComputeColumnTopY(slotPosition.y, cardSize.y);
                    float columnHeight = ComputeColumnHeight(cardSize.y, columnTopY, columnBottomY);
                    Vector2 size = ComputeSize(cardSize.x, columnHeight);
                    Vector2 offset = ComputeOffset(columnHeight, cardSize.y);
                    return (size, offset);
                }
            }
        }

        internal static class Pulse
        {
            internal static class Guards
            {
                public static bool CanPlayFoundation(SolitaireDeckConfigSO config, SolitairePileType pileType) =>
                    config != null && pileType == SolitairePileType.Foundation;

                public static bool CanPlayWin(SolitaireDeckConfigSO config) => config != null;

                public static bool CanRunRoutine(SpriteRenderer renderer) => renderer != null;
            }

            internal static class Timing
            {
                public static float EvaluateNormalizedTime(float elapsed, float duration) =>
                    Mathf.Clamp01(elapsed / duration);

                public static float EvaluateEasedProgress(float normalizedTime) =>
                    Constants.PulseFadeCompletion -
                    Mathf.Pow(Constants.PulseFadeCompletion - normalizedTime, Constants.PulseEasingPower);
            }

            internal static class ScaleOps
            {
                public static bool HasValidSize(Vector2 size) =>
                    size.x > Constants.MinPositiveSize && size.y > Constants.MinPositiveSize;

                public static Vector2 ResolveBaseSize(Vector2 rendererSize, Vector2 fallbackSize) =>
                    HasValidSize(rendererSize) ? rendererSize : fallbackSize;

                public static float Evaluate(float targetScale, float easedProgress) =>
                    Mathf.Lerp(Constants.PulseStartScale, targetScale, easedProgress);

                public static Vector3 ResolveSpriteScale(float scale) =>
                    new Vector3(scale, scale, Constants.OpaqueAlpha);

                public static Vector2 ResolveSlicedSize(Vector2 baseSize, float scale) => baseSize * scale;
            }

            internal static class SpriteOps
            {
                public static bool UsesSpriteScale(Sprite pulseSprite) => pulseSprite != null;

                public static Sprite LoadPulseSprite() =>
                    Resources.Load<Sprite>(Constants.PulseSpriteResourcePath);

                public static SpriteDrawMode ResolveDrawMode(Sprite pulseSprite) =>
                    UsesSpriteScale(pulseSprite) ? SpriteDrawMode.Simple : SpriteDrawMode.Sliced;

                public static Sprite ResolveAppliedSprite(Sprite currentSprite, Sprite pulseSprite) =>
                    pulseSprite ?? currentSprite;
            }

            internal readonly struct FrameState
            {
                public readonly float Scale;
                public readonly Color Color;
                public readonly bool UseSpriteScale;

                public FrameState(float scale, Color color, bool useSpriteScale)
                {
                    Scale = scale;
                    Color = color;
                    UseSpriteScale = useSpriteScale;
                }

                public static FrameState Compute(
                    float elapsed,
                    float duration,
                    float targetScale,
                    Color startColor,
                    bool useSpriteScale)
                {
                    float normalizedTime = Timing.EvaluateNormalizedTime(elapsed, duration);
                    float eased = Timing.EvaluateEasedProgress(normalizedTime);
                    float scale = ScaleOps.Evaluate(targetScale, eased);
                    Color color = Colors.WithFadingAlpha(startColor, normalizedTime);
                    return new FrameState(scale, color, useSpriteScale);
                }

                public static void ApplyVisual(SpriteRenderer renderer, FrameState frame, Vector2 baseSize)
                {
                    renderer.color = Colors.WithOpaqueAlpha(frame.Color);
                    ApplyScaleVisual(renderer, frame, baseSize);
                }

                private static void ApplyScaleVisual(SpriteRenderer renderer, FrameState frame, Vector2 baseSize)
                {
                    if (frame.UseSpriteScale)
                        ApplySpriteScale(renderer, frame.Scale);
                    else
                        ApplySlicedSize(renderer, baseSize, frame.Scale);
                }

                private static void ApplySpriteScale(SpriteRenderer renderer, float scale)
                {
                    renderer.transform.localScale = ScaleOps.ResolveSpriteScale(scale);
                }

                private static void ApplySlicedSize(SpriteRenderer renderer, Vector2 baseSize, float scale)
                {
                    renderer.size = ScaleOps.ResolveSlicedSize(baseSize, scale);
                }
            }

            internal readonly struct SpriteState
            {
                public readonly Sprite PreviousSprite;
                public readonly SpriteDrawMode PreviousDrawMode;
                public readonly Sprite PulseSprite;
                public readonly bool UseSpriteScale;

                public SpriteState(
                    Sprite previousSprite,
                    SpriteDrawMode previousDrawMode,
                    Sprite pulseSprite)
                {
                    PreviousSprite = previousSprite;
                    PreviousDrawMode = previousDrawMode;
                    PulseSprite = pulseSprite;
                    UseSpriteScale = SpriteOps.UsesSpriteScale(pulseSprite);
                }

                public static SpriteState Capture(SpriteRenderer renderer) =>
                    new SpriteState(renderer.sprite, renderer.drawMode, SpriteOps.LoadPulseSprite());

                public void ApplyTo(SpriteRenderer renderer)
                {
                    renderer.sprite = SpriteOps.ResolveAppliedSprite(renderer.sprite, PulseSprite);
                    renderer.drawMode = SpriteOps.ResolveDrawMode(PulseSprite);
                }

                public void RestoreTo(
                    SpriteRenderer renderer,
                    Vector2 baseSize,
                    Color defaultColor,
                    bool slotVisible)
                {
                    renderer.transform.localScale = Constants.UnitLocalScale;
                    renderer.size = baseSize;
                    renderer.sprite = PreviousSprite;
                    renderer.drawMode = PreviousDrawMode;
                    renderer.color = Colors.WithOpaqueAlpha(defaultColor);
                    renderer.enabled = slotVisible;
                }
            }
        }

        internal static class Validation
        {
            public static bool HasCollider(BoxCollider2D collider) => collider != null;

            public static string BuildMissingColliderError(string objectName) =>
                $"{objectName}{Constants.MissingColliderMessageSuffix}";

            public static (bool isValid, string error) Validate(string objectName, BoxCollider2D collider) =>
                HasCollider(collider)
                    ? (true, string.Empty)
                    : (false, BuildMissingColliderError(objectName));
        }
    }
}
