using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Input
{
    internal static class SolitaireInputLogic
    {
        public static bool CanProcessPointerInput(SolitaireDeckConfigSO config, SolitaireRuntimeContext context) =>
            config != null && context != null && !context.IsAnimationLocked;

        public static bool CanUpdateDragPresentation(SolitaireRuntimeContext context, bool hasPointerInputSource) =>
            context != null && hasPointerInputSource && context.IsDragging;

        public static bool IsActivePointer(bool isPointerDown) => isPointerDown;

        public static bool ShouldEndDragOnPointerUp(bool isDragging) => isDragging;

        public static bool HasPressedCard(int pressedCardId) => pressedCardId >= 0;

        public static bool CanBeginDrag(
            bool isPointerDown,
            bool isDragging,
            int pressedCardId,
            Vector3 pointerWorld,
            Vector3 pointerDownWorld,
            float dragStartThresholdWorld,
            bool canStartDrag) =>
            isPointerDown &&
            !isDragging &&
            HasPressedCard(pressedCardId) &&
            HasExceededDragThreshold(pointerWorld, pointerDownWorld, dragStartThresholdWorld) &&
            canStartDrag;

        public static bool HasExceededDragThreshold(Vector3 pointerWorld, Vector3 pointerDownWorld, float threshold) =>
            Vector2.Distance(pointerWorld, pointerDownWorld) >= threshold;

        public static bool CanEvaluateDrop(bool hasTarget, bool canMoveToSlot) =>
            hasTarget && canMoveToSlot;

        public static PileRef ResolveDragHighlightTarget(int pressedCardId, bool hasDropTarget, PileRef dropTarget) =>
            !HasPressedCard(pressedCardId) || !hasDropTarget ? PileRef.Invalid : dropTarget;

        public static bool IsDoubleTap(int cardId, int lastTapCardId, float lastTapTime, float currentTime, float threshold) =>
            cardId == lastTapCardId && currentTime - lastTapTime <= threshold;

        public static bool ShouldAutoMoveToFoundation(bool isDoubleTap, bool doubleTapMovesToFoundationOnly) =>
            isDoubleTap && doubleTapMovesToFoundationOnly;

        public static bool CanUseTapSelection(bool enableTapSelection) => enableTapSelection;

        public static bool ShouldTryMoveSelectionToTappedCard(bool hasSelection, int selectedCardId, int tappedCardId) =>
            hasSelection && selectedCardId != tappedCardId;

        public static bool CanSelectCardOnTap(bool canStartDrag) => canStartDrag;

        public static bool ShouldInvokeWasteCardClicked(SolitairePileType pileType) =>
            pileType == SolitairePileType.Waste;

        public static bool CanProcessSelectableSlotTap(bool enableTapSelection) => enableTapSelection;

        public static bool ShouldResetPressedCardFeedback(int pressedCardId) => HasPressedCard(pressedCardId);

        public static PileRef CreatePileRefFromCardState(CardState cardState) =>
            new PileRef(cardState.CurrentPileType, cardState.CurrentPileIndex);
    }
}
