using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    internal static partial class CardViewLogic
    {
        internal static class Constants
        {
            public const int InvalidCardId = -1;
            public const float DefaultInvalidFeedbackDuration = 0.12f;
            public const float PressedFeedbackScaleMultiplier = 1.04f;
            public const float DragShadowDepthZ = 0.01f;
            public const float SelectionHighlightDepthZ = -0.02f;
            public const float DefaultDragShadowOffsetX = 0.04f;
            public const float DefaultDragShadowOffsetY = -0.05f;
            public const float HighlightLocalScaleZ = 1f;
            public const int DragShadowSortingOffset = -1;
            public const int SelectionHighlightSortingOffset = 2;
            public const string DragShadowChildName = "DragShadow";
            public const string SelectionHighlightChildName = "SelectionHighlight";
            public const string SelectionHighlightSpriteResourcePath = "Solitaire/card_selection_highlight";

            public static readonly Color SelectionHighlightColor = new Color(0.35f, 0.72f, 1f, 1f);
        }

        internal static class Components
        {
            public static void RequireMotionPresenter(CardMotionPresenter presenter, string objectName)
            {
                if (presenter != null)
                    return;

                throw new InvalidOperationException($"{objectName} is missing CardMotionPresenter.");
            }

            public static void RequireDragBehaviour(CardDragBehaviour behaviour, string objectName)
            {
                if (behaviour != null)
                    return;

                throw new InvalidOperationException($"{objectName} is missing CardDragBehaviour.");
            }
        }

        internal static class Identity
        {
            public static int ResolveCardId(CardRuntimeIdentity identity) =>
                identity != null ? identity.CardId : Constants.InvalidCardId;

            public static bool RequiresIdentityUpdate(int currentCardId, int stateCardId) =>
                currentCardId != stateCardId;

            public static bool ShouldSync(CardRuntimeIdentity identity, int stateCardId) =>
                identity != null && RequiresIdentityUpdate(identity.CardId, stateCardId);
        }

        internal static class Guard
        {
            public static bool ShouldSkipRefresh(bool isPresenting) => isPresenting;

            public static bool ShouldSkipDragVisual(SolitaireDeckConfigSO config, bool isPresenting) =>
                config == null || isPresenting;

            public static bool ShouldSkipPressedFeedback(bool isPresenting, bool isDragVisualActive) =>
                isPresenting || isDragVisualActive;

            public static bool ShouldSkipResetFeedback(bool isPresenting) => isPresenting;

            public static bool ShouldRegisterCard(int cardId) => cardId >= 0;
        }

        internal static class HomeState
        {
            public static Vector3 ReadScale(Transform transform) => transform.localScale;

            public static Color ReadCardColor(SpriteRenderer cardRenderer) =>
                MainRenderer.IsAssigned(cardRenderer) ? cardRenderer.color : Color.white;
        }

        internal static class VisualState
        {
            public static CardVisualState ResolveIdleState(bool isFaceUp) =>
                isFaceUp ? CardVisualState.FaceUpIdle : CardVisualState.FaceDown;
        }

        internal static class Feedback
        {
            public static Vector3 ResolvePressedScale(Vector3 homeScale) =>
                homeScale * Constants.PressedFeedbackScaleMultiplier;
        }

        internal static class Visibility
        {
            public static bool ShouldClearVisuals(bool isVisible) => !isVisible;
        }

        internal static class Validation
        {
            public static bool TryValidate(
                string objectName,
                CardRuntimeIdentity identity,
                SpriteRenderer cardRenderer,
                CardMotionPresenter motionPresenter,
                out string error)
            {
                if (identity == null)
                {
                    error = $"{objectName} is missing CardRuntimeIdentity.";
                    return false;
                }

                if (cardRenderer == null)
                {
                    error = $"{objectName} is missing SpriteRenderer.";
                    return false;
                }

                if (motionPresenter == null)
                {
                    error = $"{objectName} is missing CardMotionPresenter.";
                    return false;
                }

                error = string.Empty;
                return true;
            }
        }
    }
}
