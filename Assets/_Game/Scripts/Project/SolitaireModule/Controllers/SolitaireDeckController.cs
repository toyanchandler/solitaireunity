using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireDeckController : MonoBehaviour, ISolitaireMoveQueries, ISolitaireMoveCommands
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private SolitaireMoveService _moveService;
        private SolitaireGameFlowService _gameFlowService;
        private SolitaireMovePresentationHandler _presentationHandler;
        private bool _hasAnnouncedWin;
        private int _hintCycleIndex;

        public int CurrentMoveCount => _context?.MoveHistory.Count ?? 0;
        public bool CanUndo => _context?.MoveHistory.CanUndo ?? false;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            SolitaireMoveResolver moveResolver,
            SolitaireMoveExecutor moveExecutor,
            SolitaireLayoutController layoutController,
            SolitaireHapticFeedbackProvider hapticFeedbackProvider = null)
        {
            _config = config;
            _context = context;
            _moveService = new SolitaireMoveService(moveResolver, moveExecutor);
            _gameFlowService = new SolitaireGameFlowService();
            _presentationHandler = new SolitaireMovePresentationHandler();
            _presentationHandler.Initialize(layoutController, hapticFeedbackProvider);
            _context.LayoutMetrics.ResetToConfig(_config);
            _gameFlowService.ResetFoundationTracking(_context.BoardState);
            ResetHintCycle();
            EventManager.SolitaireEvents.Ready?.Invoke();
        }

        public void StartNewDeal()
        {
            EnsureInitialized();
            ResetSessionForNewBoard();
            _gameFlowService.PrepareNewDeal(_context, _config);
            EventManager.SolitaireEvents.DealStarted?.Invoke();
            _presentationHandler.PlayInitialDeal(OnDealAnimationCompleted);
            PublishMoveCount();
        }

        public void StartDebugScenario(SolitaireDebugScenarioId scenario)
        {
            EnsureInitialized();

            SolitaireDeckCommandLogic.DebugScenarioGateResult gate =
                SolitaireDeckCommandLogic.DebugScenario.EvaluateGate(scenario);

            if (!SolitaireDeckCommandLogic.DebugScenario.IsAllowed(gate))
            {
                Debug.LogWarning(SolitaireDeckCommandLogic.DebugScenario.FormatGateWarning(gate, scenario));
                return;
            }

            ResetSessionForNewBoard();
            EndActiveDragIfNeeded();
            _gameFlowService.PrepareDebugScenario(_context, scenario);
            EventManager.SolitaireEvents.DealStarted?.Invoke();
            _presentationHandler.ApplyDebugScenarioPresentation();
            EventManager.SolitaireEvents.DealCompleted?.Invoke();
            PublishMoveCount();
            Debug.Log(SolitaireDeckCommandLogic.DebugScenario.FormatAppliedLog(scenario));
        }

        public bool TryDrawOrRecycleStock()
        {
            EnsureInitialized();

            if (!_moveService.TryDrawOrRecycleStock(_context, _config, out SolitaireStockActionResult actionResult))
                return false;

            SolitaireDeckCommandLogic.StockAction.ApplyPresentation(_presentationHandler, actionResult);
            SolitaireDeckCommandLogic.Events.PublishScoreAction(
                SolitaireDeckCommandLogic.Score.MapStockActionToScoreAction(actionResult.WasRecycle));
            PublishMoveCount();
            return true;
        }

        public bool TryShowNextHint()
        {
            EnsureInitialized();

            if (!_moveService.TryGetHint(_context.BoardState, _config, _hintCycleIndex, out SolitaireHint hint))
            {
                EventManager.SolitaireEvents.HintShown?.Invoke(SolitaireHint.None);
                return false;
            }

            _hintCycleIndex = SolitaireDeckCommandLogic.Hints.AdvanceCycleIndex(_hintCycleIndex);
            EventManager.SolitaireEvents.HintShown?.Invoke(hint);
            return true;
        }

        public int TryAutoCompleteToFoundation()
        {
            EnsureInitialized();

            int completedMoveCount = SolitaireDeckCommandLogic.AutoComplete.ExecuteSequence(
                SolitaireHintService.MaxAutoCompleteMoves,
                TryGetNextAutoCompleteMove,
                TryExecuteAutoCompleteMove);

            EventManager.SolitaireEvents.AutoCompleteCompleted?.Invoke(completedMoveCount);
            return completedMoveCount;
        }

        public bool TryMoveCardToSlot(int cardId, PileRef target)
        {
            EnsureInitialized();

            return _moveService.TryMoveCardToSlot(_context, _config, cardId, target, out SolitaireMove move, out SolitaireMoveResult result)
                && CompleteAcceptedMove(move, result);
        }

        public bool CanMoveCardToSlot(int cardId, PileRef target)
        {
            EnsureInitialized();
            return _moveService.CanMoveCardToSlot(_context.BoardState, _config, cardId, target);
        }

        public void ReturnCardToCurrentPile(int cardId)
        {
            EnsureInitialized();
            _presentationHandler.HandleReturnToPile(cardId, _context.BoardState);
        }

        public bool TryAutoMoveToFoundation(int cardId)
        {
            EnsureInitialized();

            return _moveService.TryAutoMoveToFoundation(_context, _config, cardId, out SolitaireMove move, out SolitaireMoveResult result)
                && CompleteAcceptedMove(move, result);
        }

        public bool TryFlipTableauTop(int cardId)
        {
            EnsureInitialized();

            if (!_moveService.TryFlipTableauTop(_context, _config, cardId, out SolitaireMove move, out _))
                return false;

            _presentationHandler.HandleFlipTableauTop(move.Source, cardId);
            SolitaireDeckCommandLogic.Events.PublishRevealedTableauCard(cardId);
            PublishMoveCount();
            return true;
        }

        public bool TryUndo()
        {
            EnsureInitialized();

            if (!_moveService.TryUndo(_context))
                return false;

            ApplyUndoSessionRefresh();
            return true;
        }

        public void NotifyInvalidMove()
        {
            EventManager.SolitaireEvents.InvalidMove?.Invoke();
            _presentationHandler.HandleInvalidMove();
        }

        public bool CanStartDrag(int cardId)
        {
            EnsureInitialized();
            return _moveService.CanStartDrag(_context.BoardState, _config, cardId);
        }

        public bool CanCardReceiveInput(int cardId)
        {
            EnsureInitialized();
            return _moveService.CanCardReceiveInput(_context.BoardState, _config, cardId);
        }

        private bool TryGetNextAutoCompleteMove(out SolitaireHint hint) =>
            _moveService.TryGetNextAutoCompleteMove(_context.BoardState, _config, out hint);

        private bool TryExecuteAutoCompleteMove(SolitaireHint hint) =>
            SolitaireDeckCommandLogic.AutoComplete.ResolveExecution(hint.Kind) switch
            {
                SolitaireDeckCommandLogic.AutoCompleteExecutionKind.MoveToFoundation =>
                    TryAutoMoveToFoundation(hint.Move.StartCardId),
                SolitaireDeckCommandLogic.AutoCompleteExecutionKind.MoveToSlot =>
                    TryMoveCardToSlot(hint.Move.StartCardId, hint.Move.Target),
                _ => false
            };

        private bool CompleteAcceptedMove(SolitaireMove move, SolitaireMoveResult result)
        {
            HandleAcceptedMove(move, result);
            PublishMoveCount();
            return true;
        }

        private void HandleAcceptedMove(SolitaireMove move, SolitaireMoveResult result)
        {
            _presentationHandler.HandleAcceptedMove(move, result);
            ResetHintCycle();
            SolitaireDeckCommandLogic.Events.PublishAcceptedMoveScoreActions(move, result);
            PublishFoundationProgress();
            EventManager.SolitaireEvents.MoveCompleted?.Invoke(move);
            TryAnnounceWin();
        }

        private void ApplyUndoSessionRefresh()
        {
            _hasAnnouncedWin = _gameFlowService.IsWon(_context.BoardState);
            _context.SelectionState.Clear();
            _presentationHandler.HandleUndo();
            _gameFlowService.ResetFoundationTracking(_context.BoardState);
            ResetHintCycle();
            SolitaireDeckCommandLogic.Events.PublishScoreAction(SolitaireScoreAction.Undo);
            PublishMoveCount();
        }

        private void ResetSessionForNewBoard()
        {
            _hasAnnouncedWin = false;
            ResetHintCycle();
            _context.SelectionState.Clear();
        }

        private void EndActiveDragIfNeeded()
        {
            if (!SolitaireDeckCommandLogic.Runtime.ShouldEndActiveDrag(_context))
                return;

            _context.EndDrag();
        }

        private void OnDealAnimationCompleted()
        {
            EventManager.SolitaireEvents.DealCompleted?.Invoke();
        }

        private void ResetHintCycle()
        {
            _hintCycleIndex = SolitaireDeckCommandLogic.Hints.ResetCycleIndex();
        }

        private void TryAnnounceWin()
        {
            if (!SolitaireDeckCommandLogic.Win.ShouldAnnounce(_hasAnnouncedWin, _gameFlowService.IsWon(_context.BoardState)))
                return;

            _hasAnnouncedWin = true;
            _presentationHandler.PlayWinCelebration();
            EventManager.SolitaireEvents.GameWon?.Invoke();
        }

        private void PublishFoundationProgress()
        {
            FoundationProgressDelta[] changes = _gameFlowService.CollectFoundationProgressChanges(_context.BoardState);
            SolitaireDeckCommandLogic.FoundationProgress.PublishChanges(changes);
        }

        private void EnsureInitialized()
        {
            if (SolitaireDeckCommandLogic.Runtime.IsReady(_config, _context, _moveService, _presentationHandler))
                return;

            throw new InvalidOperationException($"{nameof(SolitaireDeckController)} is not initialized.");
        }

        private void PublishMoveCount()
        {
            SolitaireDeckCommandLogic.Events.PublishMoveCount(CurrentMoveCount, CanUndo);
        }
    }
}
