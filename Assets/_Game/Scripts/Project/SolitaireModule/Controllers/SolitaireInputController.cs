using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Input;
using _Game.Scripts.Project.SolitaireModule.Presentation;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireInputController : MonoBehaviour
    {
        private readonly SolitaireBoardHitTester _hitTester = new SolitaireBoardHitTester();
        private readonly SolitaireDropTargetResolver _dropTargetResolver = new SolitaireDropTargetResolver();
        private readonly SolitaireDragPresenter _dragPresenter = new SolitaireDragPresenter();
        private readonly Action<Vector3>[] _pointerPhaseHandlers = new Action<Vector3>[4];
        private readonly Action<CardInputReceiver>[] _cardPressHandlers = new Action<CardInputReceiver>[4];
        private readonly Action<SolitaireSlotAnchor>[] _slotPressHandlers = new Action<SolitaireSlotAnchor>[4];

        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private ISolitaireMoveQueries _moveQueries;
        private ISolitaireMoveCommands _moveCommands;
        private SolitairePointerInputSource _pointerInputSource;
        private SolitaireHapticFeedbackProvider _hapticFeedbackProvider;
        private Transform _dragParent;
        private int _pressedCardId = -1;
        private float _lastTapTime = -10f;
        private int _lastTapCardId = -1;
        private bool _isPointerDown;
        private Vector3 _pointerDownWorld;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            SolitaireDeckController deckController,
            SolitairePointerInputSource pointerInputSource,
            SolitaireHapticFeedbackProvider hapticFeedbackProvider)
        {
            _config = config;
            _context = context;
            _moveQueries = deckController;
            _moveCommands = deckController;
            _pointerInputSource = pointerInputSource;
            _hapticFeedbackProvider = hapticFeedbackProvider;
            _dropTargetResolver.Initialize(config, context, _moveQueries, _hitTester);
            _dragPresenter.Initialize(config, context, _moveQueries);
            BindPointerHandlers();
            BindPressHandlers();
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.DragLayerReady += HandleDragLayerReady;
            EventManager.SolitaireEvents.DealStarted += HandleDealStarted;

            if (SolitaireFeatureRegistration.DragLayer != null)
                HandleDragLayerReady(SolitaireFeatureRegistration.DragLayer);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.DragLayerReady -= HandleDragLayerReady;
            EventManager.SolitaireEvents.DealStarted -= HandleDealStarted;
            _dragParent = null;
        }

        private void HandleDealStarted()
        {
            if (_context != null)
                SolitaireCardSelectionVisuals.ClearAll(_context);
        }

        private void HandleDragLayerReady(Transform dragLayer)
        {
            _dragParent = dragLayer;
        }

        private void Update()
        {
            if (_config == null || _context == null || _context.IsAnimationLocked)
                return;

            if (!_pointerInputSource.TryGetPointer(out Vector3 pointerWorld, out SolitairePointerPhase phase))
                return;

            _pointerPhaseHandlers[(int)phase]?.Invoke(pointerWorld);
        }

        private void LateUpdate()
        {
            if (_context == null || _pointerInputSource == null || !_context.IsDragging)
                return;

            if (_pointerInputSource.TryGetPointer(out Vector3 pointerWorld, out _))
            {
                _dragPresenter.MoveDraggedCards(pointerWorld);
                UpdateDragTargetHighlight(pointerWorld);
            }
        }

        private void BindPointerHandlers()
        {
            _pointerPhaseHandlers[(int)SolitairePointerPhase.Down] = HandlePointerDown;
            _pointerPhaseHandlers[(int)SolitairePointerPhase.Hold] = HandlePointerHold;
            _pointerPhaseHandlers[(int)SolitairePointerPhase.Up] = HandlePointerUp;
        }

        private void BindPressHandlers()
        {
            _cardPressHandlers[(int)SolitairePileType.Stock] = _ => _moveCommands.TryDrawOrRecycleStock();
            _cardPressHandlers[(int)SolitairePileType.Waste] = HandleSelectableCardPress;
            _cardPressHandlers[(int)SolitairePileType.Foundation] = HandleSelectableCardPress;
            _cardPressHandlers[(int)SolitairePileType.Tableau] = HandleSelectableCardPress;

            _slotPressHandlers[(int)SolitairePileType.Stock] = _ => _moveCommands.TryDrawOrRecycleStock();
            _slotPressHandlers[(int)SolitairePileType.Waste] = HandleSelectableSlotPress;
            _slotPressHandlers[(int)SolitairePileType.Foundation] = HandleSelectableSlotPress;
            _slotPressHandlers[(int)SolitairePileType.Tableau] = HandleSelectableSlotPress;
        }

        private void HandlePointerDown(Vector3 pointerWorld)
        {
            _isPointerDown = true;
            _pointerDownWorld = pointerWorld;
            _pressedCardId = -1;

            CardInputReceiver card = _hitTester.GetCardUnderPointer(pointerWorld, _moveQueries);

            if (card != null)
            {
                CardState state = _context.BoardState.GetCard(card.Identity.CardId);
                _cardPressHandlers[(int)state.CurrentPileType]?.Invoke(card);
                return;
            }

            SolitaireSlotAnchor slot = _hitTester.GetSlotUnderPointer(pointerWorld);

            if (slot == null)
                return;

            _slotPressHandlers[(int)slot.PileType]?.Invoke(slot);
        }

        private void HandleSelectableCardPress(CardInputReceiver card)
        {
            _pressedCardId = card.Identity.CardId;
            CardState state = _context.BoardState.GetCard(_pressedCardId);

            if (state.CurrentPileType == SolitairePileType.Waste)
                EventManager.SolitaireEvents.WasteCardClicked?.Invoke();

            card.View.PlayPressedFeedback();
            _hapticFeedbackProvider.PlayLight();
        }

        private void HandleSelectableSlotPress(SolitaireSlotAnchor slot)
        {
            if (slot == null || !_config.EnableTapSelection)
                return;

            TryMoveSelectionToSlot(slot.PileRef);
        }

        private void HandlePointerHold(Vector3 pointerWorld)
        {
            bool canBeginDrag = _isPointerDown &&
                                !_context.IsDragging &&
                                _pressedCardId >= 0 &&
                                Vector2.Distance(pointerWorld, _pointerDownWorld) >= _config.DragStartThresholdWorld &&
                                _moveQueries.CanStartDrag(_pressedCardId);

            if (!canBeginDrag)
                return;

            BeginDrag(pointerWorld);
        }

        private void HandlePointerUp(Vector3 pointerWorld)
        {
            if (!_isPointerDown)
                return;

            if (_context.IsDragging)
            {
                EndDrag(pointerWorld);
                ResetPointerState();
                return;
            }

            if (_pressedCardId >= 0)
                HandleTap(_pressedCardId);

            ResetPointerState();
        }

        private void BeginDrag(Vector3 pointerWorld)
        {
            _dragPresenter.BeginDrag(_pressedCardId, _pointerDownWorld, _dragParent);
            EventManager.SolitaireEvents.CardHoldStarted?.Invoke();
            _dragPresenter.MoveDraggedCards(pointerWorld);
            UpdateDragTargetHighlight(pointerWorld);
        }

        private void EndDrag(Vector3 pointerWorld)
        {
            bool hasTarget = TryGetDropTargetUnderPointer(pointerWorld, out PileRef target);
            bool moved = hasTarget &&
                         _moveQueries.CanMoveCardToSlot(_pressedCardId, target) &&
                         _moveCommands.TryMoveCardToSlot(_pressedCardId, target);

            if (moved)
            {
                EventManager.SolitaireEvents.CardDropSucceeded?.Invoke();
                _context.SelectionState.Clear();
            }
            else
            {
                EventManager.SolitaireEvents.CardDropFailed?.Invoke();
                CardView pressed = _context.ViewRegistry.GetCard(_pressedCardId);
                pressed.ResetFeedback();
                pressed.PlayInvalidFeedback(_config.InvalidMoveReturnDuration);
                _moveCommands.ReturnCardToCurrentPile(_pressedCardId);
                _moveCommands.NotifyInvalidMove();
            }

            _dragPresenter.FinishDrag();
        }

        private void HandleTap(int cardId)
        {
            CardView card = _context.ViewRegistry.GetCard(cardId);
            card.ResetFeedback();

            bool isDoubleTap = cardId == _lastTapCardId && Time.time - _lastTapTime <= _config.DoubleTapThreshold;
            _lastTapTime = Time.time;
            _lastTapCardId = cardId;

            if (isDoubleTap && _config.DoubleTapMovesToFoundationOnly)
            {
                bool moved = _moveCommands.TryAutoMoveToFoundation(cardId);
                _context.SelectionState.Clear();

                if (!moved)
                {
                    card.PlayInvalidFeedback(_config.InvalidMoveReturnDuration);
                    _moveCommands.NotifyInvalidMove();
                }

                return;
            }

            bool flipped = _moveCommands.TryFlipTableauTop(cardId);

            if (flipped)
            {
                _context.SelectionState.Clear();
                return;
            }

            bool selected = _config.EnableTapSelection && HandleTapSelection(cardId);

            if (!selected)
                card.ResetFeedback();
        }

        private bool TryGetDropTargetUnderPointer(Vector3 pointerWorld, out PileRef target)
        {
            return _dropTargetResolver.TryGetDropTarget(
                pointerWorld,
                _pressedCardId,
                _context.IsDragging,
                _dragPresenter.DraggedCount,
                _dragPresenter.IsDraggedCard,
                out target);
        }

        private void UpdateDragTargetHighlight(Vector3 pointerWorld)
        {
            PileRef highlightTarget = _pressedCardId < 0
                ? PileRef.Invalid
                : TryGetDropTargetUnderPointer(pointerWorld, out PileRef target) ? target : PileRef.Invalid;

            _dragPresenter.UpdateTargetHighlight(highlightTarget);
        }

        private bool HandleTapSelection(int cardId)
        {
            if (_context.SelectionState.HasSelection && _context.SelectionState.SelectedCardId != cardId)
            {
                CardState targetState = _context.BoardState.GetCard(cardId);

                if (TryMoveSelectionToSlot(new PileRef(targetState.CurrentPileType, targetState.CurrentPileIndex)))
                    return true;
            }

            if (!_moveQueries.CanStartDrag(cardId))
            {
                _context.SelectionState.Clear();
                return false;
            }

            _context.SelectionState.Select(cardId);
            return true;
        }

        private bool TryMoveSelectionToSlot(PileRef target)
        {
            if (!_context.SelectionState.HasSelection)
                return false;

            int selectedCardId = _context.SelectionState.SelectedCardId;

            if (!_moveCommands.TryMoveCardToSlot(selectedCardId, target))
                return false;

            _context.SelectionState.Clear();
            return true;
        }

        private void ResetPointerState()
        {
            if (_pressedCardId >= 0)
                _context.ViewRegistry.GetCard(_pressedCardId).ResetFeedback();

            _dragPresenter.ClearDropTargetHighlights();
            _isPointerDown = false;
            _pressedCardId = -1;
        }
    }
}
