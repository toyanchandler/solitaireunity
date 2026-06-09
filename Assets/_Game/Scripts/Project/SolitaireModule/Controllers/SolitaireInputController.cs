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
            if (_context == null)
                return;

            SolitaireCardSelectionVisuals.ClearAll(_context);
        }

        private void HandleDragLayerReady(Transform dragLayer)
        {
            _dragParent = dragLayer;
        }

        private void Update()
        {
            if (!SolitaireInputLogic.CanProcessPointerInput(_config, _context))
                return;

            if (!_pointerInputSource.TryGetPointer(out Vector3 pointerWorld, out SolitairePointerPhase phase))
                return;

            _pointerPhaseHandlers[(int)phase]?.Invoke(pointerWorld);
        }

        private void LateUpdate()
        {
            if (!SolitaireInputLogic.CanUpdateDragPresentation(_context, _pointerInputSource != null))
                return;

            if (!_pointerInputSource.TryGetPointer(out Vector3 pointerWorld, out _))
                return;

            _dragPresenter.MoveDraggedCards(pointerWorld);
            UpdateDragTargetHighlight(pointerWorld);
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
            BeginPointerDown(pointerWorld);
            DispatchPointerPress(pointerWorld);
        }

        private void BeginPointerDown(Vector3 pointerWorld)
        {
            _isPointerDown = true;
            _pointerDownWorld = pointerWorld;
            _pressedCardId = -1;
        }

        private void DispatchPointerPress(Vector3 pointerWorld)
        {
            CardInputReceiver card = _hitTester.GetCardUnderPointer(pointerWorld, _moveQueries);

            if (card != null)
            {
                DispatchCardPress(card);
                return;
            }

            SolitaireSlotAnchor slot = _hitTester.GetSlotUnderPointer(pointerWorld);

            if (slot == null)
                return;

            DispatchSlotPress(slot);
        }

        private void DispatchCardPress(CardInputReceiver card)
        {
            CardState state = _context.BoardState.GetCard(card.Identity.CardId);
            _cardPressHandlers[(int)state.CurrentPileType]?.Invoke(card);
        }

        private void DispatchSlotPress(SolitaireSlotAnchor slot)
        {
            _slotPressHandlers[(int)slot.PileType]?.Invoke(slot);
        }

        private void HandleSelectableCardPress(CardInputReceiver card)
        {
            _pressedCardId = card.Identity.CardId;
            CardState state = _context.BoardState.GetCard(_pressedCardId);
            RaiseWasteCardClickedIfNeeded(state.CurrentPileType);
            card.View.PlayPressedFeedback();
            _hapticFeedbackProvider.PlayLight();
        }

        private void RaiseWasteCardClickedIfNeeded(SolitairePileType pileType)
        {
            if (!SolitaireInputLogic.ShouldInvokeWasteCardClicked(pileType))
                return;

            EventManager.SolitaireEvents.WasteCardClicked?.Invoke();
        }

        private void HandleSelectableSlotPress(SolitaireSlotAnchor slot)
        {
            if (!SolitaireInputLogic.CanProcessSelectableSlotTap(_config.EnableTapSelection))
                return;

            TryMoveSelectionToSlot(slot.PileRef);
        }

        private void HandlePointerHold(Vector3 pointerWorld)
        {
            if (!CanBeginDrag(pointerWorld))
                return;

            BeginDrag(pointerWorld);
        }

        private bool CanBeginDrag(Vector3 pointerWorld) =>
            SolitaireInputLogic.CanBeginDrag(
                _isPointerDown,
                _context.IsDragging,
                _pressedCardId,
                pointerWorld,
                _pointerDownWorld,
                _config.DragStartThresholdWorld,
                _moveQueries.CanStartDrag(_pressedCardId));

        private void HandlePointerUp(Vector3 pointerWorld)
        {
            if (!SolitaireInputLogic.IsActivePointer(_isPointerDown))
                return;

            if (SolitaireInputLogic.ShouldEndDragOnPointerUp(_context.IsDragging))
            {
                EndDrag(pointerWorld);
                ResetPointerState();
                return;
            }

            if (SolitaireInputLogic.HasPressedCard(_pressedCardId))
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
            if (TryExecuteDrop(pointerWorld))
                HandleDropSucceeded();
            else
                HandleDropFailed();

            _dragPresenter.FinishDrag();
        }

        private bool TryExecuteDrop(Vector3 pointerWorld)
        {
            bool hasTarget = TryGetDropTargetUnderPointer(pointerWorld, out PileRef target);
            bool canMove = _moveQueries.CanMoveCardToSlot(_pressedCardId, target);

            return SolitaireInputLogic.CanEvaluateDrop(hasTarget, canMove) &&
                   _moveCommands.TryMoveCardToSlot(_pressedCardId, target);
        }

        private void HandleDropSucceeded()
        {
            EventManager.SolitaireEvents.CardDropSucceeded?.Invoke();
            _context.SelectionState.Clear();
        }

        private void HandleDropFailed()
        {
            EventManager.SolitaireEvents.CardDropFailed?.Invoke();
            CardView pressed = _context.ViewRegistry.GetCard(_pressedCardId);
            pressed.ResetFeedback();
            pressed.PlayInvalidFeedback(_config.InvalidMoveReturnDuration);
            _moveCommands.ReturnCardToCurrentPile(_pressedCardId);
            _moveCommands.NotifyInvalidMove();
        }

        private void HandleTap(int cardId)
        {
            CardView card = _context.ViewRegistry.GetCard(cardId);
            card.ResetFeedback();

            bool isDoubleTap = RecordTapTiming(cardId);

            if (TryHandleDoubleTapFoundation(cardId, card, isDoubleTap))
                return;

            if (TryHandleTableauFlip(cardId))
                return;

            if (!TryHandleTapSelection(cardId))
                card.ResetFeedback();
        }

        private bool RecordTapTiming(int cardId)
        {
            bool isDoubleTap = SolitaireInputLogic.IsDoubleTap(
                cardId,
                _lastTapCardId,
                _lastTapTime,
                Time.time,
                _config.DoubleTapThreshold);

            _lastTapTime = Time.time;
            _lastTapCardId = cardId;
            return isDoubleTap;
        }

        private bool TryHandleDoubleTapFoundation(int cardId, CardView card, bool isDoubleTap)
        {
            if (!SolitaireInputLogic.ShouldAutoMoveToFoundation(isDoubleTap, _config.DoubleTapMovesToFoundationOnly))
                return false;

            bool moved = _moveCommands.TryAutoMoveToFoundation(cardId);
            _context.SelectionState.Clear();

            if (moved)
                return true;

            card.PlayInvalidFeedback(_config.InvalidMoveReturnDuration);
            _moveCommands.NotifyInvalidMove();
            return true;
        }

        private bool TryHandleTableauFlip(int cardId)
        {
            if (!_moveCommands.TryFlipTableauTop(cardId))
                return false;

            _context.SelectionState.Clear();
            return true;
        }

        private bool TryHandleTapSelection(int cardId)
        {
            if (!SolitaireInputLogic.CanUseTapSelection(_config.EnableTapSelection))
                return false;

            return HandleTapSelection(cardId);
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
            bool hasTarget = TryGetDropTargetUnderPointer(pointerWorld, out PileRef target);
            PileRef highlightTarget = SolitaireInputLogic.ResolveDragHighlightTarget(_pressedCardId, hasTarget, target);
            _dragPresenter.UpdateTargetHighlight(highlightTarget);
        }

        private bool HandleTapSelection(int cardId)
        {
            if (TryMoveSelectionToTappedCard(cardId))
                return true;

            return TrySelectCard(cardId);
        }

        private bool TryMoveSelectionToTappedCard(int cardId)
        {
            if (!SolitaireInputLogic.ShouldTryMoveSelectionToTappedCard(
                    _context.SelectionState.HasSelection,
                    _context.SelectionState.SelectedCardId,
                    cardId))
                return false;

            CardState targetState = _context.BoardState.GetCard(cardId);
            return TryMoveSelectionToSlot(SolitaireInputLogic.CreatePileRefFromCardState(targetState));
        }

        private bool TrySelectCard(int cardId)
        {
            if (!SolitaireInputLogic.CanSelectCardOnTap(_moveQueries.CanStartDrag(cardId)))
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
            if (SolitaireInputLogic.ShouldResetPressedCardFeedback(_pressedCardId))
                _context.ViewRegistry.GetCard(_pressedCardId).ResetFeedback();

            _dragPresenter.ClearDropTargetHighlights();
            _isPointerDown = false;
            _pressedCardId = -1;
        }
    }
}
