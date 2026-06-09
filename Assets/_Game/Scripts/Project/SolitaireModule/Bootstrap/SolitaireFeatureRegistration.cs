using System;
using System.Collections.Generic;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;
using RegistrationLogic = _Game.Scripts.Project.SolitaireModule.Runtime.SolitaireFeatureRegistrationLogic;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    /// <summary>
    /// Collects self-registered scene objects and publishes announcements through EventManager.SolitaireEvents.
    /// </summary>
    public static class SolitaireFeatureRegistration
    {
        private static readonly CardView[] RegisteredCards = new CardView[SolitaireCardUtility.CardCount];
        private static readonly List<SolitaireSlotAnchor> RegisteredSlots = new List<SolitaireSlotAnchor>(13);

        public static SolitaireBoardCameraController BoardCamera { get; private set; }
        public static Transform DragLayer { get; private set; }
        public static SolitaireModuleControllerBundle ControllerHost { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Reset();

        public static void Reset()
        {
            BoardCamera = null;
            DragLayer = null;
            ControllerHost = null;
            Array.Clear(RegisteredCards, 0, RegisteredCards.Length);
            RegisteredSlots.Clear();
            EventManager.SolitaireEvents.Reset();
        }

        public static void RegisterControllerHost(SolitaireModuleControllerBundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            if (RegistrationLogic.ControllerHost.HasConflict(ControllerHost, bundle))
                throw new InvalidOperationException("A different Solitaire ControllerHost is already registered.");

            ControllerHost = bundle;
        }

        public static void UnregisterControllerHost(SolitaireModuleControllerBundle bundle)
        {
            if (RegistrationLogic.SingleRegistration.ShouldClear(ControllerHost, bundle))
                ControllerHost = null;
        }

        public static bool TryGetControllerHost(out SolitaireModuleControllerBundle bundle, out string error)
        {
            if (!RegistrationLogic.ControllerHost.IsRegistered(ControllerHost))
            {
                bundle = null;
                error = RegistrationLogic.ControllerHost.MissingError;
                return false;
            }

            bundle = ControllerHost;
            error = string.Empty;
            return true;
        }

        public static void RegisterBoardCamera(SolitaireBoardCameraController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            if (RegistrationLogic.BoardCamera.HasConflict(BoardCamera, controller))
                throw new InvalidOperationException("A different Solitaire board camera is already registered.");

            BoardCamera = controller;
            EventManager.SolitaireEvents.BoardCameraReady?.Invoke(controller);
        }

        public static void UnregisterBoardCamera(SolitaireBoardCameraController controller)
        {
            if (RegistrationLogic.SingleRegistration.ShouldClear(BoardCamera, controller))
                BoardCamera = null;
        }

        public static void RegisterDragLayer(Transform dragLayer)
        {
            if (dragLayer == null)
                throw new ArgumentNullException(nameof(dragLayer));

            if (RegistrationLogic.DragLayer.HasConflict(DragLayer, dragLayer))
                throw new InvalidOperationException("A different Solitaire drag layer is already registered.");

            DragLayer = dragLayer;
            EventManager.SolitaireEvents.DragLayerReady?.Invoke(dragLayer);
        }

        public static void UnregisterDragLayer(Transform dragLayer)
        {
            if (RegistrationLogic.SingleRegistration.ShouldClear(DragLayer, dragLayer))
                DragLayer = null;
        }

        public static void RegisterCard(CardView card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if (!RegistrationLogic.CardId.IsInRange(card.CardId, RegisteredCards.Length))
                throw new InvalidOperationException(
                    RegistrationLogic.CardId.FormatInvalidMessage(card.name, card.CardId));

            CardView registeredCard = RegisteredCards[card.CardId];

            if (RegistrationLogic.CardRegistration.HasDuplicate(registeredCard, card))
                throw new InvalidOperationException(
                    RegistrationLogic.CardId.FormatDuplicateMessage(
                        card.CardId,
                        registeredCard.name,
                        card.name));

            RegisteredCards[card.CardId] = card;
            EventManager.SolitaireEvents.CardRegistered?.Invoke(card);
        }

        public static void UnregisterCard(CardView card)
        {
            if (!RegistrationLogic.CardRegistration.ShouldClearOnUnregister(RegisteredCards, card))
                return;

            RegisteredCards[card.CardId] = null;
        }

        public static void RegisterSlot(SolitaireSlotAnchor slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            SolitaireSlotAnchor registeredSlot = FindRegisteredSlot(slot.PileRef);

            if (RegistrationLogic.SlotRegistration.HasDuplicate(registeredSlot, slot))
                throw new InvalidOperationException(
                    RegistrationLogic.SlotRegistration.FormatDuplicateMessage(registeredSlot, slot));

            if (RegistrationLogic.SlotRegistration.ShouldAddToList(RegisteredSlots, slot))
                RegisteredSlots.Add(slot);

            EventManager.SolitaireEvents.SlotRegistered?.Invoke(slot);
        }

        public static void UnregisterSlot(SolitaireSlotAnchor slot)
        {
            if (slot == null)
                return;

            RegisteredSlots.Remove(slot);
        }

        public static void NotifyBoardViewportSizeChanged() =>
            EventManager.SolitaireEvents.BoardViewportSizeChanged?.Invoke();

        public static bool TryCreateViewRegistry(out SolitaireViewRegistry registry, out string error)
        {
            registry = new SolitaireViewRegistry();

            var prerequisiteFailure = RegistrationLogic.ViewRegistryBuild.EvaluatePrerequisites(
                BoardCamera,
                DragLayer);

            if (prerequisiteFailure != RegistrationLogic.ViewRegistryBuild.PrerequisiteFailure.None)
            {
                error = RegistrationLogic.ViewRegistryBuild.FormatPrerequisiteError(prerequisiteFailure);
                return false;
            }

            if (!RegistrationLogic.ViewRegistryBuild.TryRegisterAllCards(RegisteredCards, registry, out error))
                return false;

            RegistrationLogic.ViewRegistryBuild.RegisterAllSlots(RegisteredSlots, registry);
            return registry.Validate(out error);
        }

        public static IReadOnlyList<SolitaireSlotAnchor> GetRegisteredSlotsSnapshot() =>
            RegisteredSlots.ToArray();

        public static bool TryGetRegisteredCard(int cardId, out CardView card) =>
            RegistrationLogic.RegisteredCardLookup.TryGetCard(RegisteredCards, cardId, out card);

        public static bool TryGetRegisteredSlot(PileRef pileRef, out SolitaireSlotAnchor slot)
        {
            slot = FindRegisteredSlot(pileRef);
            return slot != null;
        }

        private static SolitaireSlotAnchor FindRegisteredSlot(PileRef pileRef) =>
            RegistrationLogic.SlotRegistration.FindByPileRef(RegisteredSlots, pileRef);
    }
}
