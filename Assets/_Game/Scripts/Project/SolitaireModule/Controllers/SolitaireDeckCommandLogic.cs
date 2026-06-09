using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    internal static class SolitaireDeckCommandLogic
    {
        internal enum DebugScenarioGateResult
        {
            Allowed = 0,
            RejectedNone = 1,
            RejectedFlowScenario = 2
        }

        internal enum AutoCompleteExecutionKind
        {
            None = 0,
            MoveToFoundation = 1,
            MoveToSlot = 2
        }

        internal delegate bool TryGetAutoCompleteMove(out SolitaireHint hint);
        internal delegate bool TryExecuteAutoCompleteMove(SolitaireHint hint);

        internal static class Runtime
        {
            public static bool IsReady(
                SolitaireDeckConfigSO config,
                SolitaireRuntimeContext context,
                SolitaireMoveService moveService,
                SolitaireMovePresentationHandler presentationHandler) =>
                config != null &&
                context != null &&
                moveService != null &&
                presentationHandler != null;

            public static bool ShouldEndActiveDrag(SolitaireRuntimeContext context) =>
                context != null && context.IsDragging;
        }

        internal static class DebugScenario
        {
            public static DebugScenarioGateResult EvaluateGate(SolitaireDebugScenarioId scenario) =>
                scenario switch
                {
                    SolitaireDebugScenarioId.None => DebugScenarioGateResult.RejectedNone,
                    _ when SolitaireDebugScenarioApplier.IsFlowScenario(scenario) =>
                        DebugScenarioGateResult.RejectedFlowScenario,
                    _ => DebugScenarioGateResult.Allowed
                };

            public static string FormatGateWarning(
                DebugScenarioGateResult gateResult,
                SolitaireDebugScenarioId scenario) =>
                gateResult switch
                {
                    DebugScenarioGateResult.RejectedNone =>
                        "[SolitaireDeckController] Debug scenario is None.",
                    DebugScenarioGateResult.RejectedFlowScenario =>
                        $"[SolitaireDeckController] Flow scenario '{scenario}' must be applied through bootstrap flow handler.",
                    _ => string.Empty
                };

            public static bool IsAllowed(DebugScenarioGateResult gateResult) =>
                gateResult == DebugScenarioGateResult.Allowed;

            public static string FormatAppliedLog(SolitaireDebugScenarioId scenario) =>
                $"[SolitaireDeckController] temp — Debug scenario board refreshed: {scenario}";
        }

        internal static class Hints
        {
            public static int ResetCycleIndex() => 0;

            public static int AdvanceCycleIndex(int currentIndex) => currentIndex + 1;
        }

        internal static class Score
        {
            public static SolitaireScoreAction MapMoveTypeToScoreAction(SolitaireMoveType moveType) =>
                moveType switch
                {
                    SolitaireMoveType.WasteToFoundation => SolitaireScoreAction.MoveToFoundation,
                    SolitaireMoveType.TableauToFoundation => SolitaireScoreAction.MoveToFoundation,
                    SolitaireMoveType.AutoMoveToFoundation => SolitaireScoreAction.MoveToFoundation,
                    _ => SolitaireScoreAction.MoveToTableau
                };

            public static SolitaireScoreAction MapStockActionToScoreAction(bool wasRecycle) =>
                wasRecycle
                    ? SolitaireScoreAction.StockRecycle
                    : SolitaireScoreAction.StockDraw;
        }

        internal static class Moves
        {
            public static bool HasRevealedTableauCard(int revealedCardId) => revealedCardId >= 0;
        }

        internal static class Win
        {
            public static bool ShouldAnnounce(bool hasAnnouncedWin, bool isWon) =>
                !hasAnnouncedWin && isWon;
        }

        internal static class AutoComplete
        {
            public static bool ShouldContinue(int iteration, int maxMoves) => iteration < maxMoves;

            public static AutoCompleteExecutionKind ResolveExecution(SolitaireHintKind kind) =>
                kind switch
                {
                    SolitaireHintKind.MoveToFoundation => AutoCompleteExecutionKind.MoveToFoundation,
                    SolitaireHintKind.WasteToTableau => AutoCompleteExecutionKind.MoveToSlot,
                    SolitaireHintKind.RevealTableauByMove => AutoCompleteExecutionKind.MoveToSlot,
                    SolitaireHintKind.TableauToTableau => AutoCompleteExecutionKind.MoveToSlot,
                    _ => AutoCompleteExecutionKind.None
                };

            public static int ExecuteSequence(
                int maxMoves,
                TryGetAutoCompleteMove tryGetMove,
                TryExecuteAutoCompleteMove tryExecuteMove)
            {
                int completedMoveCount = 0;

                for (int i = 0; ShouldContinue(i, maxMoves); i++)
                {
                    if (!tryGetMove(out SolitaireHint hint))
                        break;

                    if (!tryExecuteMove(hint))
                        break;

                    completedMoveCount++;
                }

                return completedMoveCount;
            }
        }

        internal static class StockAction
        {
            public static void ApplyPresentation(
                SolitaireMovePresentationHandler presentationHandler,
                SolitaireStockActionResult result)
            {
                if (result.WasRecycle)
                    ApplyRecyclePresentation(presentationHandler);
                else
                    ApplyDrawPresentation(presentationHandler, result.DrawnCardId);
            }

            private static void ApplyRecyclePresentation(SolitaireMovePresentationHandler presentationHandler)
            {
                presentationHandler.HandleWasteRecycle();
                EventManager.SolitaireEvents.WasteRecycled?.Invoke();
            }

            private static void ApplyDrawPresentation(
                SolitaireMovePresentationHandler presentationHandler,
                int drawnCardId)
            {
                presentationHandler.HandleStockDraw(drawnCardId);
                EventManager.SolitaireEvents.StockDrawn?.Invoke(drawnCardId);
                EventManager.SolitaireEvents.StockDrawClicked?.Invoke();
            }
        }

        internal static class FoundationProgress
        {
            public static void PublishChanges(FoundationProgressDelta[] changes)
            {
                for (int i = 0; i < changes.Length; i++)
                {
                    FoundationProgressDelta change = changes[i];
                    EventManager.SolitaireEvents.FoundationProgressChanged?.Invoke(
                        change.FoundationIndex,
                        change.CardCount);
                }
            }
        }

        internal static class Events
        {
            public static void PublishScoreAction(SolitaireScoreAction action) =>
                EventManager.SolitaireEvents.ScoreActionPerformed?.Invoke(action);

            public static void PublishMoveCount(int moveCount, bool canUndo)
            {
                EventManager.SolitaireEvents.MoveCountChanged?.Invoke(moveCount);
                EventManager.SolitaireEvents.UndoAvailabilityChanged?.Invoke(canUndo);
            }

            public static void PublishRevealedTableauCard(int revealedCardId)
            {
                EventManager.SolitaireEvents.CardFlipped?.Invoke(revealedCardId);
                PublishScoreAction(SolitaireScoreAction.RevealTableauCard);
            }

            public static void PublishAcceptedMoveScoreActions(SolitaireMove move, SolitaireMoveResult result)
            {
                PublishScoreAction(Score.MapMoveTypeToScoreAction(move.Type));

                if (!Moves.HasRevealedTableauCard(result.RevealedCardId))
                    return;

                PublishRevealedTableauCard(result.RevealedCardId);
            }
        }
    }
}
