using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Input
{
    public sealed class SolitaireBoardHitTester
    {
        private static readonly bool[] DropTargetPileTypes = CreateDropTargetPileTypes();
        private readonly Collider2D[] _hitBuffer = new Collider2D[32];

        public CardInputReceiver GetCardUnderPointer(Vector3 pointerWorld, ISolitaireMoveQueries moveQueries)
        {
            int count = Physics2D.OverlapPoint(pointerWorld, ContactFilter2D.noFilter, _hitBuffer);
            CardInputReceiver best = null;
            int bestOrder = int.MinValue;

            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].TryGetComponent(out CardInputReceiver card))
                    continue;

                if (!moveQueries.CanCardReceiveInput(card.Identity.CardId))
                    continue;

                int order = Mathf.RoundToInt(-card.View.CachedTransform.position.z * 10000f);

                if (order <= bestOrder)
                    continue;

                bestOrder = order;
                best = card;
            }

            return best;
        }

        public SolitaireSlotAnchor GetSlotUnderPointer(Vector3 pointerWorld)
        {
            int count = Physics2D.OverlapPoint(pointerWorld, ContactFilter2D.noFilter, _hitBuffer);
            SolitaireSlotAnchor bestSlot = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].TryGetComponent(out SolitaireSlotAnchor slot))
                    continue;

                float distance = Vector2.Distance(pointerWorld, slot.transform.position);

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestSlot = slot;
            }

            return bestSlot;
        }

        public bool TryGetSlotUnderPointer(Vector3 pointerWorld, out SolitaireSlotAnchor slot)
        {
            slot = GetSlotUnderPointer(pointerWorld);
            return slot != null;
        }

        public bool TryGetCardDropTargetAtPoint(
            Vector3 point,
            SolitaireRuntimeContext context,
            Func<int, bool> isIgnoredCard,
            out PileRef target)
        {
            int count = Physics2D.OverlapPoint(point, ContactFilter2D.noFilter, _hitBuffer);
            CardInputReceiver bestCard = null;
            int bestOrder = int.MinValue;

            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].TryGetComponent(out CardInputReceiver card))
                    continue;

                if (isIgnoredCard(card.Identity.CardId))
                    continue;

                CardState state = context.BoardState.GetCard(card.Identity.CardId);

                if (!DropTargetPileTypes[(int)state.CurrentPileType])
                    continue;

                int order = Mathf.RoundToInt(-card.View.CachedTransform.position.z * 10000f);

                if (order <= bestOrder)
                    continue;

                bestOrder = order;
                bestCard = card;
            }

            if (bestCard != null)
            {
                CardState state = context.BoardState.GetCard(bestCard.Identity.CardId);
                target = new PileRef(state.CurrentPileType, state.CurrentPileIndex);
                return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private static bool[] CreateDropTargetPileTypes()
        {
            var lookup = new bool[4];
            lookup[(int)SolitairePileType.Foundation] = true;
            lookup[(int)SolitairePileType.Tableau] = true;
            return lookup;
        }
    }
}
