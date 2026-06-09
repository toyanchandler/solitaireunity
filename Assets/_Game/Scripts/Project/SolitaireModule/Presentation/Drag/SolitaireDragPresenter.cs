using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitaireDragPresenter
    {
        private readonly int[] _draggedCardIds = new int[SolitaireCardUtility.CardCount];
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private ISolitaireMoveQueries _moveQueries;
        private SolitaireSlotAnchor _highlightedSlot;
        private int _draggedCount;
        private int _pressedCardId = -1;

        public int DraggedCount => _draggedCount;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            ISolitaireMoveQueries moveQueries)
        {
            _config = config;
            _context = context;
            _moveQueries = moveQueries;
        }

        public void BeginDrag(int pressedCardId, Vector3 pointerDownWorld, Transform dragParent)
        {
            _pressedCardId = pressedCardId;
            _context.BeginDrag();
            BuildDraggedCards(pressedCardId);

            for (int i = 0; i < _draggedCount; i++)
            {
                CardView card = _context.ViewRegistry.GetCard(_draggedCardIds[i]);
                card.BeginDrag(dragParent, pointerDownWorld, _config, _config.DragSortingOrder + i);
            }

            HighlightPossibleDropTargets();
        }

        public void MoveDraggedCards(Vector3 pointerWorld)
        {
            for (int i = 0; i < _draggedCount; i++)
            {
                CardView card = _context.ViewRegistry.GetCard(_draggedCardIds[i]);
                card.MoveDrag(pointerWorld);
            }
        }

        public void FinishDrag()
        {
            for (int i = 0; i < _draggedCount; i++)
            {
                CardView card = _context.ViewRegistry.GetCard(_draggedCardIds[i]);
                card.FinishDrag();
            }

            ClearDropTargetHighlights();
            _context.EndDrag();
            _draggedCount = 0;
            _pressedCardId = -1;
        }

        public void UpdateTargetHighlight(PileRef target)
        {
            SolitaireSlotAnchor slot = target.IsValid ? _context.ViewRegistry.GetSlot(target) : null;
            SetHighlightedSlot(slot);
        }

        public void ClearDropTargetHighlights()
        {
            SetHighlightedSlot(null);
            ClearDropTargetHighlights(_context.ViewRegistry.Tableaus);
            ClearDropTargetHighlights(_context.ViewRegistry.Foundations);
        }

        public bool IsDraggedCard(int cardId)
        {
            for (int i = 0; i < _draggedCount; i++)
            {
                if (_draggedCardIds[i] == cardId)
                    return true;
            }

            return false;
        }

        private void BuildDraggedCards(int startCardId)
        {
            _draggedCount = 0;
            CardState startCard = _context.BoardState.GetCard(startCardId);
            FixedCardPileState sourcePile = _context.BoardState.GetPile(new PileRef(startCard.CurrentPileType, startCard.CurrentPileIndex));
            int startIndex = sourcePile.IndexOf(startCardId);

            if (startIndex < 0)
                return;

            for (int i = startIndex; i < sourcePile.Count; i++)
            {
                _draggedCardIds[_draggedCount] = sourcePile[i];
                _draggedCount++;
            }
        }

        private void SetHighlightedSlot(SolitaireSlotAnchor slot)
        {
            if (_highlightedSlot == slot)
                return;

            if (_highlightedSlot != null)
                _highlightedSlot.SetHighlight(false, _config.ValidTargetHighlightColor);

            _highlightedSlot = slot;

            if (_highlightedSlot != null)
                _highlightedSlot.SetHighlight(true, _config.ValidTargetHighlightColor);
        }

        private void HighlightPossibleDropTargets()
        {
            if (_pressedCardId < 0)
                return;

            HighlightPossibleDropTargets(_context.ViewRegistry.Tableaus);
            HighlightPossibleDropTargets(_context.ViewRegistry.Foundations);
        }

        private void HighlightPossibleDropTargets(SolitaireSlotAnchor[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                SolitaireSlotAnchor slot = slots[i];

                if (slot == null)
                    continue;

                bool isPossible = _moveQueries.CanMoveCardToSlot(_pressedCardId, slot.PileRef);
                slot.SetHighlight(isPossible, _config.ValidTargetHighlightColor);
            }
        }

        private void ClearDropTargetHighlights(SolitaireSlotAnchor[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                SolitaireSlotAnchor slot = slots[i];

                if (slot != null)
                    slot.SetHighlight(false, _config.ValidTargetHighlightColor);
            }
        }
    }
}
