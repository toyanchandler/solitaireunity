using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Input
{
    public sealed class SolitaireDropTargetResolver
    {
        private readonly Vector3[] _dropProbePoints = new Vector3[5];
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private ISolitaireMoveQueries _moveQueries;
        private SolitaireBoardHitTester _hitTester;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            ISolitaireMoveQueries moveQueries,
            SolitaireBoardHitTester hitTester)
        {
            _config = config;
            _context = context;
            _moveQueries = moveQueries;
            _hitTester = hitTester;
        }

        public bool TryGetDropTarget(
            Vector3 pointerWorld,
            int pressedCardId,
            bool isDragging,
            int draggedCount,
            Func<int, bool> isDraggedCard,
            out PileRef target)
        {
            int probeCount = BuildDropProbePoints(pointerWorld, pressedCardId, isDragging, draggedCount);

            if (TryGetTableauColumnDropTargetFromProbePoints(probeCount, pressedCardId, out target))
                return true;

            if (TryGetDropTargetFromProbePoints(probeCount, pressedCardId, isDraggedCard, out target))
                return true;

            if (TryGetFoundationDropTargetFromProbePoints(probeCount, pressedCardId, out target))
                return true;

            if (TryGetNearestDropTargetFromProbePoints(probeCount, pressedCardId, out target))
                return true;

            target = PileRef.Invalid;
            return false;
        }

        private int BuildDropProbePoints(Vector3 pointerWorld, int pressedCardId, bool isDragging, int draggedCount)
        {
            _dropProbePoints[0] = pointerWorld;

            if (!isDragging || pressedCardId < 0 || draggedCount <= 0)
                return 1;

            Vector3 center = _context.ViewRegistry.GetCard(pressedCardId).CachedTransform.position;
            center.z = 0f;

            Vector2 cardSize = _context.LayoutMetrics.CardSize;
            float halfWidth = cardSize.x * 0.5f;
            float halfHeight = cardSize.y * 0.5f;
            float horizontalOffset = _config.DragProbeHorizontalOffsetRatio;
            float cornerYOffset = _config.DragProbeCornerYOffsetRatio;
            _dropProbePoints[1] = center + new Vector3(0f, halfHeight * _config.DragProbeTopOffsetRatio, 0f);
            _dropProbePoints[2] = center + new Vector3(-halfWidth * horizontalOffset, halfHeight * cornerYOffset, 0f);
            _dropProbePoints[3] = center + new Vector3(halfWidth * horizontalOffset, halfHeight * cornerYOffset, 0f);
            _dropProbePoints[4] = center;
            return _dropProbePoints.Length;
        }

        private bool TryGetDropTargetFromProbePoints(
            int probeCount,
            int pressedCardId,
            Func<int, bool> isDraggedCard,
            out PileRef target)
        {
            for (int i = 0; i < probeCount; i++)
            {
                Vector3 probePoint = _dropProbePoints[i];
                SolitaireSlotAnchor slot = _hitTester.GetSlotUnderPointer(probePoint);

                if (slot != null && TrySelectDropCandidate(pressedCardId, slot.PileRef, out target))
                    return true;

                if (_hitTester.TryGetCardDropTargetAtPoint(probePoint, _context, isDraggedCard, out PileRef cardTarget) &&
                    TrySelectDropCandidate(pressedCardId, cardTarget, out target))
                {
                    return true;
                }
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetFoundationDropTargetFromProbePoints(int probeCount, int pressedCardId, out PileRef target)
        {
            for (int i = 0; i < probeCount; i++)
            {
                if (TryGetFoundationDropTargetAtPoint(_dropProbePoints[i], pressedCardId, out target))
                    return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetFoundationDropTargetAtPoint(Vector3 point, int pressedCardId, out PileRef target)
        {
            SolitaireSlotAnchor[] foundations = _context.ViewRegistry.Foundations;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;
            float halfWidth = cardSize.x * _config.FoundationDropHalfWidthRatio;
            float halfHeight = cardSize.y * _config.FoundationDropHalfHeightRatio;
            SolitaireSlotAnchor bestSlot = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < foundations.Length; i++)
            {
                SolitaireSlotAnchor slot = foundations[i];

                if (slot == null || !_moveQueries.CanMoveCardToSlot(pressedCardId, slot.PileRef))
                    continue;

                Vector3 slotPosition = slot.transform.position;

                if (Mathf.Abs(point.x - slotPosition.x) > halfWidth ||
                    Mathf.Abs(point.y - slotPosition.y) > halfHeight)
                {
                    continue;
                }

                float distance = Vector2.Distance(point, slotPosition);

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestSlot = slot;
            }

            if (bestSlot != null)
            {
                target = bestSlot.PileRef;
                return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetTableauColumnDropTargetFromProbePoints(int probeCount, int pressedCardId, out PileRef target)
        {
            for (int i = 0; i < probeCount; i++)
            {
                if (!TryGetTableauColumnAtPoint(_dropProbePoints[i], out PileRef tableauTarget))
                    continue;

                if (TrySelectDropCandidate(pressedCardId, tableauTarget, out target))
                    return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetTableauColumnAtPoint(Vector3 point, out PileRef target)
        {
            SolitaireSlotAnchor[] tableaus = _context.ViewRegistry.Tableaus;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;
            float halfWidth = cardSize.x * _config.TableauColumnHalfWidthRatio;
            float topPadding = cardSize.y * _config.TableauColumnTopPaddingRatio;
            float bottomY = _context.LayoutMetrics.TableauBottomPlayableY -
                            cardSize.y * _config.TableauColumnBottomPaddingRatio;
            SolitaireSlotAnchor bestSlot = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < tableaus.Length; i++)
            {
                SolitaireSlotAnchor slot = tableaus[i];

                if (slot == null)
                    continue;

                BoxCollider2D collider = slot.BoxCollider;

                if (collider != null && collider.enabled)
                {
                    if (!collider.OverlapPoint(point))
                        continue;
                }
                else
                {
                    Vector3 slotPosition = slot.transform.position;

                    if (point.y > slotPosition.y + topPadding || point.y < bottomY)
                        continue;

                    if (Mathf.Abs(point.x - slotPosition.x) > halfWidth)
                        continue;
                }

                float distanceX = Mathf.Abs(point.x - slot.transform.position.x);

                if (distanceX >= bestDistance)
                    continue;

                bestDistance = distanceX;
                bestSlot = slot;
            }

            if (bestSlot != null)
            {
                target = bestSlot.PileRef;
                return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetNearestDropTargetFromProbePoints(int probeCount, int pressedCardId, out PileRef target)
        {
            for (int i = 0; i < probeCount; i++)
            {
                if (!TryGetNearestDropSlot(_dropProbePoints[i], out SolitaireSlotAnchor slot))
                    continue;

                if (TrySelectDropCandidate(pressedCardId, slot.PileRef, out target))
                    return true;
            }

            target = PileRef.Invalid;
            return false;
        }

        private bool TryGetNearestDropSlot(Vector3 pointerWorld, out SolitaireSlotAnchor nearestSlot)
        {
            nearestSlot = null;
            float bestDistance = _config.DropSnapDistance;

            TrySelectNearestSlot(_context.ViewRegistry.Tableaus, pointerWorld, ref nearestSlot, ref bestDistance);
            TrySelectNearestSlot(_context.ViewRegistry.Foundations, pointerWorld, ref nearestSlot, ref bestDistance);
            return nearestSlot != null;
        }

        private static void TrySelectNearestSlot(
            SolitaireSlotAnchor[] slots,
            Vector3 pointerWorld,
            ref SolitaireSlotAnchor nearestSlot,
            ref float bestDistance)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                SolitaireSlotAnchor slot = slots[i];

                if (slot == null)
                    continue;

                float distance = Vector2.Distance(pointerWorld, slot.transform.position);

                if (distance > bestDistance)
                    continue;

                bestDistance = distance;
                nearestSlot = slot;
            }
        }

        private bool TrySelectDropCandidate(int pressedCardId, PileRef candidate, out PileRef target)
        {
            if (pressedCardId >= 0 && _moveQueries.CanMoveCardToSlot(pressedCardId, candidate))
            {
                target = candidate;
                return true;
            }

            target = PileRef.Invalid;
            return false;
        }
    }
}
