using System.Collections.Generic;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static class SolitaireFeatureRegistrationLogic
    {
        internal static class SingleRegistration
        {
            public static bool HasConflict<T>(T existing, T candidate) where T : class =>
                existing != null && existing != candidate;

            public static bool ShouldClear<T>(T registered, T candidate) where T : class =>
                ReferenceEquals(registered, candidate);
        }

        internal static class ControllerHost
        {
            public const string MissingError =
                "ControllerHost is not registered. Add SolitaireModuleControllerHost to ControllerHost.";

            public static bool IsRegistered(SolitaireModuleControllerBundle bundle) =>
                bundle != null;

            public static bool HasConflict(
                SolitaireModuleControllerBundle existing,
                SolitaireModuleControllerBundle candidate) =>
                SingleRegistration.HasConflict(existing, candidate);
        }

        internal static class BoardCamera
        {
            public static bool HasConflict(
                SolitaireBoardCameraController existing,
                SolitaireBoardCameraController candidate) =>
                SingleRegistration.HasConflict(existing, candidate);
        }

        internal static class DragLayer
        {
            public static bool HasConflict(Transform existing, Transform candidate) =>
                SingleRegistration.HasConflict(existing, candidate);
        }

        internal static class CardId
        {
            public static bool IsInRange(int cardId, int cardCount) =>
                (uint)cardId < (uint)cardCount;

            public static string FormatInvalidMessage(string cardName, int cardId) =>
                $"{cardName} has invalid CardId {cardId}.";

            public static string FormatDuplicateMessage(int cardId, string existingName, string candidateName) =>
                $"Duplicate Solitaire CardId {cardId:00}: {existingName} and {candidateName}.";
        }

        internal static class CardRegistration
        {
            public static bool HasDuplicate(CardView existing, CardView candidate) =>
                SingleRegistration.HasConflict(existing, candidate);

            public static bool ShouldClearOnUnregister(CardView[] registeredCards, CardView candidate) =>
                candidate != null &&
                CardId.IsInRange(candidate.CardId, registeredCards.Length) &&
                ReferenceEquals(registeredCards[candidate.CardId], candidate);
        }

        internal static class SlotRegistration
        {
            public static bool HasDuplicate(SolitaireSlotAnchor existing, SolitaireSlotAnchor candidate) =>
                SingleRegistration.HasConflict(existing, candidate);

            public static bool ShouldAddToList(IReadOnlyList<SolitaireSlotAnchor> registeredSlots, SolitaireSlotAnchor candidate) =>
                candidate != null && !ContainsSlot(registeredSlots, candidate);

            public static bool MatchesPileRef(SolitaireSlotAnchor slot, PileRef pileRef) =>
                slot != null &&
                slot.PileType == pileRef.Type &&
                slot.PileIndex == pileRef.Index;

            public static string FormatDuplicateMessage(
                SolitaireSlotAnchor existing,
                SolitaireSlotAnchor candidate) =>
                $"Duplicate Solitaire slot registration for {candidate.PileType} {candidate.PileIndex}: {existing.name} and {candidate.name}.";

            public static SolitaireSlotAnchor FindByPileRef(
                IReadOnlyList<SolitaireSlotAnchor> registeredSlots,
                PileRef pileRef)
            {
                for (int i = 0; i < registeredSlots.Count; i++)
                {
                    SolitaireSlotAnchor slot = registeredSlots[i];

                    if (MatchesPileRef(slot, pileRef))
                        return slot;
                }

                return null;
            }

            private static bool ContainsSlot(IReadOnlyList<SolitaireSlotAnchor> registeredSlots, SolitaireSlotAnchor candidate)
            {
                for (int i = 0; i < registeredSlots.Count; i++)
                {
                    if (ReferenceEquals(registeredSlots[i], candidate))
                        return true;
                }

                return false;
            }
        }

        internal static class RegisteredCardLookup
        {
            public static bool TryGetCard(CardView[] registeredCards, int cardId, out CardView card)
            {
                card = null;

                if (!CardId.IsInRange(cardId, registeredCards.Length))
                    return false;

                card = registeredCards[cardId];
                return card != null;
            }
        }

        internal static class ViewRegistryBuild
        {
            internal enum PrerequisiteFailure
            {
                None = 0,
                MissingBoardCamera = 1,
                MissingDragLayer = 2
            }

            public const string MissingBoardCameraError =
                "No board camera registered. Add SolitaireBoardCameraController to the feature camera.";

            public const string MissingDragLayerError =
                "No drag layer registered. Add SolitaireDragLayer to DragParent.";

            public static bool HasBoardCamera(SolitaireBoardCameraController boardCamera) =>
                boardCamera != null;

            public static bool HasDragLayer(Transform dragLayer) =>
                dragLayer != null;

            public static PrerequisiteFailure EvaluatePrerequisites(
                SolitaireBoardCameraController boardCamera,
                Transform dragLayer) =>
                !HasBoardCamera(boardCamera) ? PrerequisiteFailure.MissingBoardCamera
                : !HasDragLayer(dragLayer) ? PrerequisiteFailure.MissingDragLayer
                : PrerequisiteFailure.None;

            public static string FormatPrerequisiteError(PrerequisiteFailure failure) =>
                failure switch
                {
                    PrerequisiteFailure.MissingBoardCamera => MissingBoardCameraError,
                    PrerequisiteFailure.MissingDragLayer => MissingDragLayerError,
                    _ => string.Empty
                };

            public static bool IsCardPresent(CardView card) =>
                card != null;

            public static string FormatMissingCardMessage(int cardIndex) =>
                $"Missing registered Card_{cardIndex:00}.";

            public static bool TryRegisterCardAtIndex(
                CardView[] registeredCards,
                int cardIndex,
                SolitaireViewRegistry registry,
                out string error)
            {
                CardView card = registeredCards[cardIndex];

                if (!IsCardPresent(card))
                {
                    error = FormatMissingCardMessage(cardIndex);
                    return false;
                }

                if (!card.Validate(out error))
                    return false;

                registry.RegisterCard(card);
                error = string.Empty;
                return true;
            }

            public static bool TryRegisterAllCards(
                CardView[] registeredCards,
                SolitaireViewRegistry registry,
                out string error)
            {
                for (int i = 0; i < registeredCards.Length; i++)
                {
                    if (!TryRegisterCardAtIndex(registeredCards, i, registry, out error))
                        return false;
                }

                error = string.Empty;
                return true;
            }

            public static void RegisterAllSlots(
                IReadOnlyList<SolitaireSlotAnchor> registeredSlots,
                SolitaireViewRegistry registry)
            {
                for (int i = 0; i < registeredSlots.Count; i++)
                    registry.RegisterSlot(registeredSlots[i]);
            }
        }
    }
}
