using System;
using System.Collections.Generic;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

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
        private static void ResetStatics()
        {
            Reset();
        }

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

            if (ControllerHost != null && ControllerHost != bundle)
                throw new InvalidOperationException("A different Solitaire ControllerHost is already registered.");

            ControllerHost = bundle;
        }

        public static void UnregisterControllerHost(SolitaireModuleControllerBundle bundle)
        {
            if (ControllerHost == bundle)
                ControllerHost = null;
        }

        public static bool TryGetControllerHost(out SolitaireModuleControllerBundle bundle, out string error)
        {
            if (ControllerHost == null)
            {
                bundle = null;
                error = "ControllerHost is not registered. Add SolitaireModuleControllerHost to ControllerHost.";
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

            if (BoardCamera != null && BoardCamera != controller)
                throw new InvalidOperationException("A different Solitaire board camera is already registered.");

            BoardCamera = controller;
            EventManager.SolitaireEvents.BoardCameraReady?.Invoke(controller);
        }

        public static void UnregisterBoardCamera(SolitaireBoardCameraController controller)
        {
            if (BoardCamera == controller)
                BoardCamera = null;
        }

        public static void RegisterDragLayer(Transform dragLayer)
        {
            if (dragLayer == null)
                throw new ArgumentNullException(nameof(dragLayer));

            if (DragLayer != null && DragLayer != dragLayer)
                throw new InvalidOperationException("A different Solitaire drag layer is already registered.");

            DragLayer = dragLayer;
            EventManager.SolitaireEvents.DragLayerReady?.Invoke(dragLayer);
        }

        public static void UnregisterDragLayer(Transform dragLayer)
        {
            if (DragLayer == dragLayer)
                DragLayer = null;
        }

        public static void RegisterCard(CardView card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if ((uint)card.CardId >= (uint)RegisteredCards.Length)
                throw new InvalidOperationException($"{card.name} has invalid CardId {card.CardId}.");

            CardView registeredCard = RegisteredCards[card.CardId];

            if (registeredCard != null && registeredCard != card)
                throw new InvalidOperationException($"Duplicate Solitaire CardId {card.CardId:00}: {registeredCard.name} and {card.name}.");

            RegisteredCards[card.CardId] = card;
            EventManager.SolitaireEvents.CardRegistered?.Invoke(card);
        }

        public static void UnregisterCard(CardView card)
        {
            if (card == null)
                return;

            if ((uint)card.CardId < (uint)RegisteredCards.Length && RegisteredCards[card.CardId] == card)
                RegisteredCards[card.CardId] = null;
        }

        public static void RegisterSlot(SolitaireSlotAnchor slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            SolitaireSlotAnchor registeredSlot = FindRegisteredSlot(slot.PileRef);

            if (registeredSlot != null && registeredSlot != slot)
                throw new InvalidOperationException($"Duplicate Solitaire slot registration for {slot.PileType} {slot.PileIndex}: {registeredSlot.name} and {slot.name}.");

            if (!RegisteredSlots.Contains(slot))
                RegisteredSlots.Add(slot);

            EventManager.SolitaireEvents.SlotRegistered?.Invoke(slot);
        }

        public static void UnregisterSlot(SolitaireSlotAnchor slot)
        {
            if (slot == null)
                return;

            RegisteredSlots.Remove(slot);
        }

        public static void NotifyBoardViewportSizeChanged()
        {
            EventManager.SolitaireEvents.BoardViewportSizeChanged?.Invoke();
        }

        public static bool TryCreateViewRegistry(out SolitaireViewRegistry registry, out string error)
        {
            registry = new SolitaireViewRegistry();

            if (BoardCamera == null)
            {
                error = "No board camera registered. Add SolitaireBoardCameraController to the feature camera.";
                return false;
            }

            if (DragLayer == null)
            {
                error = "No drag layer registered. Add SolitaireDragLayer to DragParent.";
                return false;
            }

            for (int i = 0; i < RegisteredCards.Length; i++)
            {
                CardView card = RegisteredCards[i];

                if (card == null)
                {
                    error = $"Missing registered Card_{i:00}.";
                    return false;
                }

                if (!card.Validate(out error))
                    return false;

                registry.RegisterCard(card);
            }

            for (int i = 0; i < RegisteredSlots.Count; i++)
                registry.RegisterSlot(RegisteredSlots[i]);

            if (!registry.Validate(out error))
                return false;

            error = string.Empty;
            return true;
        }

        public static IReadOnlyList<SolitaireSlotAnchor> GetRegisteredSlotsSnapshot()
        {
            return RegisteredSlots.ToArray();
        }

        public static bool TryGetRegisteredCard(int cardId, out CardView card)
        {
            if ((uint)cardId >= (uint)RegisteredCards.Length)
            {
                card = null;
                return false;
            }

            card = RegisteredCards[cardId];
            return card != null;
        }

        public static bool TryGetRegisteredSlot(PileRef pileRef, out SolitaireSlotAnchor slot)
        {
            slot = FindRegisteredSlot(pileRef);
            return slot != null;
        }

        private static SolitaireSlotAnchor FindRegisteredSlot(PileRef pileRef)
        {
            for (int i = 0; i < RegisteredSlots.Count; i++)
            {
                SolitaireSlotAnchor slot = RegisteredSlots[i];

                if (slot != null && slot.PileType == pileRef.Type && slot.PileIndex == pileRef.Index)
                    return slot;
            }

            return null;
        }
    }
}
