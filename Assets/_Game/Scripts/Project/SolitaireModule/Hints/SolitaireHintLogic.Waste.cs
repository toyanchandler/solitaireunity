using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class WasteToTableauHints
        {
            public static bool TryCreateForTarget(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                int cardId,
                int targetIndex,
                out SolitaireHint hint)
            {
                SolitaireMove move = MoveFactory.CreateWasteToTableau(cardId, targetIndex);

                if (!Execution.CanExecute(board, resolver, config, move))
                    return HintResults.Fail(out hint);

                hint = new SolitaireHint(SolitaireHintKind.WasteToTableau, move);
                return true;
            }

            public static int AppendAllFromBoard(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count)
            {
                int cardId = board.Waste.PeekTop();

                if (!CardQueries.HasTopCard(cardId))
                    return count;

                return AppendAllTargets(board, config, resolver, target, count, cardId);
            }

            public static bool TryFindFirst(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint)
            {
                int cardId = board.Waste.PeekTop();

                if (!CardQueries.HasTopCard(cardId))
                    return HintResults.Fail(out hint);

                return TryFindFirstTarget(board, config, resolver, cardId, out hint);
            }

            private static int AppendAllTargets(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count,
                int cardId)
            {
                for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
                {
                    if (TryCreateForTarget(board, config, resolver, cardId, targetIndex, out SolitaireHint hint))
                        HintCollection.AppendIfUnique(target, ref count, hint);
                }

                return count;
            }

            private static bool TryFindFirstTarget(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                int cardId,
                out SolitaireHint hint)
            {
                for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
                {
                    if (TryCreateForTarget(board, config, resolver, cardId, targetIndex, out hint))
                        return true;
                }

                return HintResults.Fail(out hint);
            }
        }
    }
}
