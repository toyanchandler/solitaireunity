using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
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

        public event Action Ready;
        public event Action DealStarted;
        public event Action DealCompleted;
        public event Action<SolitaireMove> MoveCompleted;
        public event Action InvalidMove;
        public event Action<int> CardFlipped;
        public event Action<int> StockDrawn;
        public event Action WasteRecycled;
        public event Action<int, int> FoundationProgressChanged;
        public event Action<int> MoveCountChanged;
        public event Action<bool> UndoAvailabilityChanged;
        public event Action<SolitaireHint> HintShown;
        public event Action<int> AutoCompleteCompleted;
        public event Action GameWon;

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
            Ready?.Invoke();
        }

        public void StartNewDeal()
        {
            EnsureInitialized();
            _hasAnnouncedWin = false;
            ResetHintCycle();
            _gameFlowService.PrepareNewDeal(_context, _config);
            PublishDealStarted();
            _presentationHandler.PlayInitialDeal(OnDealAnimationCompleted);
            PublishMoveCount();
        }

        public void StartDebugScenario(SolitaireDebugScenarioId scenario)
        {
            EnsureInitialized();

            if (scenario == SolitaireDebugScenarioId.None)
            {
                Debug.LogWarning("[SolitaireDeckController] Debug scenario is None.");
                return;
            }

            if (SolitaireDebugScenarioApplier.IsFlowScenario(scenario))
            {
                Debug.LogWarning($"[SolitaireDeckController] Flow scenario '{scenario}' must be applied through bootstrap flow handler.");
                return;
            }

            _hasAnnouncedWin = false;
            ResetHintCycle();
            _context.SelectionState.Clear();

            if (_context.IsDragging)
                _context.EndDrag();

            _gameFlowService.PrepareDebugScenario(_context, scenario);
            PublishDealStarted();
            _presentationHandler.ApplyDebugScenarioPresentation();
            DealCompleted?.Invoke();
            PublishMoveCount();
            Debug.Log($"[SolitaireDeckController] temp — Debug scenario board refreshed: {scenario}");
        }

        public bool TryDrawOrRecycleStock()
        {
            EnsureInitialized();

            if (!_moveService.TryDrawOrRecycleStock(_context, _config, out SolitaireStockActionResult actionResult))
                return false;

            if (actionResult.WasRecycle)
            {
                _presentationHandler.HandleWasteRecycle();
                WasteRecycled?.Invoke();
                PublishScoreAction(SolitaireScoreAction.StockRecycle);
            }
            else
            {
                _presentationHandler.HandleStockDraw(actionResult.DrawnCardId);
                StockDrawn?.Invoke(actionResult.DrawnCardId);
                EventManager.SolitaireEvents.StockDrawClicked?.Invoke();
                PublishScoreAction(SolitaireScoreAction.StockDraw);
            }

            PublishMoveCount();
            return true;
        }

        public bool TryShowNextHint()
        {
            EnsureInitialized();

            if (!_moveService.TryGetHint(_context.BoardState, _config, _hintCycleIndex, out SolitaireHint hint))
            {
                HintShown?.Invoke(SolitaireHint.None);
                EventManager.SolitaireEvents.HintShown?.Invoke(SolitaireHint.None);
                return false;
            }

            _hintCycleIndex++;
            HintShown?.Invoke(hint);
            EventManager.SolitaireEvents.HintShown?.Invoke(hint);
            return true;
        }

        public int TryAutoCompleteToFoundation()
        {
            EnsureInitialized();

            int completedMoveCount = 0;

            for (int i = 0; i < SolitaireHintService.MaxAutoCompleteMoves; i++)
            {
                if (!_moveService.TryGetNextAutoCompleteMove(_context.BoardState, _config, out SolitaireHint hint))
                    break;

                if (!TryAutoMoveToFoundation(hint.Move.StartCardId))
                    break;

                completedMoveCount++;
            }

            AutoCompleteCompleted?.Invoke(completedMoveCount);
            EventManager.SolitaireEvents.AutoCompleteCompleted?.Invoke(completedMoveCount);
            return completedMoveCount;
        }

        public bool TryMoveCardToSlot(int cardId, PileRef target)
        {
            EnsureInitialized();

            if (!_moveService.TryMoveCardToSlot(_context, _config, cardId, target, out SolitaireMove move, out SolitaireMoveResult result))
                return false;

            HandleAcceptedMove(move, result);
            PublishMoveCount();
            return true;
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

            if (!_moveService.TryAutoMoveToFoundation(_context, _config, cardId, out SolitaireMove move, out SolitaireMoveResult result))
                return false;

            HandleAcceptedMove(move, result);
            PublishMoveCount();
            return true;
        }

        public bool TryFlipTableauTop(int cardId)
        {
            EnsureInitialized();

            if (!_moveService.TryFlipTableauTop(_context, _config, cardId, out SolitaireMove move, out SolitaireMoveResult result))
                return false;

            _presentationHandler.HandleFlipTableauTop(move.Source, cardId);
            CardFlipped?.Invoke(cardId);
            PublishScoreAction(SolitaireScoreAction.RevealTableauCard);
            PublishMoveCount();
            return true;
        }

        public bool TryUndo()
        {
            EnsureInitialized();

            if (!_moveService.TryUndo(_context))
                return false;

            _hasAnnouncedWin = _gameFlowService.IsWon(_context.BoardState);
            _context.SelectionState.Clear();
            _presentationHandler.HandleUndo();
            _gameFlowService.ResetFoundationTracking(_context.BoardState);
            ResetHintCycle();
            PublishScoreAction(SolitaireScoreAction.Undo);
            PublishMoveCount();
            return true;
        }

        public void NotifyInvalidMove()
        {
            InvalidMove?.Invoke();
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

        private void HandleAcceptedMove(SolitaireMove move, SolitaireMoveResult result)
        {
            _presentationHandler.HandleAcceptedMove(move, result);
            ResetHintCycle();
            PublishScoreAction(GetScoreAction(move.Type));

            if (result.RevealedCardId >= 0)
            {
                CardFlipped?.Invoke(result.RevealedCardId);
                PublishScoreAction(SolitaireScoreAction.RevealTableauCard);
            }

            PublishFoundationProgress();
            MoveCompleted?.Invoke(move);
            TryAnnounceWin();
        }

        private void OnDealAnimationCompleted()
        {
            DealCompleted?.Invoke();
        }

        private void PublishDealStarted()
        {
            DealStarted?.Invoke();
            EventManager.SolitaireEvents.DealStarted?.Invoke();
        }

        private static SolitaireScoreAction GetScoreAction(SolitaireMoveType moveType)
        {
            return moveType switch
            {
                SolitaireMoveType.WasteToFoundation => SolitaireScoreAction.MoveToFoundation,
                SolitaireMoveType.TableauToFoundation => SolitaireScoreAction.MoveToFoundation,
                SolitaireMoveType.AutoMoveToFoundation => SolitaireScoreAction.MoveToFoundation,
                _ => SolitaireScoreAction.MoveToTableau
            };
        }

        private static void PublishScoreAction(SolitaireScoreAction action)
        {
            EventManager.SolitaireEvents.ScoreActionPerformed?.Invoke(action);
        }

        private void ResetHintCycle()
        {
            _hintCycleIndex = 0;
        }

        private void TryAnnounceWin()
        {
            if (_hasAnnouncedWin || !_gameFlowService.IsWon(_context.BoardState))
                return;

            _hasAnnouncedWin = true;
            _presentationHandler.PlayWinCelebration();
            GameWon?.Invoke();
        }

        private void PublishFoundationProgress()
        {
            FoundationProgressDelta[] changes = _gameFlowService.CollectFoundationProgressChanges(_context.BoardState);

            for (int i = 0; i < changes.Length; i++)
                FoundationProgressChanged?.Invoke(changes[i].FoundationIndex, changes[i].CardCount);
        }

        private void EnsureInitialized()
        {
            if (_config == null || _context == null || _moveService == null || _presentationHandler == null)
                throw new InvalidOperationException($"{nameof(SolitaireDeckController)} is not initialized.");
        }

        private void PublishMoveCount()
        {
            MoveCountChanged?.Invoke(CurrentMoveCount);
            UndoAvailabilityChanged?.Invoke(CanUndo);
        }
    }
}
