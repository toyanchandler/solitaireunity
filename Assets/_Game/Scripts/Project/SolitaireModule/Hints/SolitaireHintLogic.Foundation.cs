using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class FoundationHints
        {
            public static bool TryCreateFromPile(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                FixedCardPileState pile,
                out SolitaireHint hint)
            {
                int cardId = pile.PeekTop();

                if (!CardQueries.HasTopCard(cardId))
                    return HintResults.Fail(out hint);

                SolitaireMove move = resolver.ResolveAutoFoundationMove(board, cardId);

                if (!Execution.CanExecute(board, resolver, config, move))
                    return HintResults.Fail(out hint);

                hint = new SolitaireHint(SolitaireHintKind.MoveToFoundation, move);
                return true;
            }

            public static bool TryAppendFromPile(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                FixedCardPileState pile,
                SolitaireHint[] target,
                ref int count) =>
                TryCreateFromPile(board, config, resolver, pile, out SolitaireHint hint) &&
                HintCollection.TryAppend(target, ref count, hint);

            public static int AppendAllFromBoard(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count)
            {
                TryAppendFromPile(board, config, resolver, board.Waste, target, ref count);
                count = AppendAllFromTableaus(board, config, resolver, target, count);
                return count;
            }

            public static bool TryFindFirst(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint)
            {
                if (TryCreateFromPile(board, config, resolver, board.Waste, out hint))
                    return true;

                return TryFindFirstInTableaus(board, config, resolver, out hint);
            }

            private static int AppendAllFromTableaus(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count)
            {
                for (int i = 0; i < board.Tableaus.Length; i++)
                    TryAppendFromPile(board, config, resolver, board.Tableaus[i], target, ref count);

                return count;
            }

            private static bool TryFindFirstInTableaus(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint)
            {
                for (int i = 0; i < board.Tableaus.Length; i++)
                {
                    if (TryCreateFromPile(board, config, resolver, board.Tableaus[i], out hint))
                        return true;
                }

                return HintResults.Fail(out hint);
            }
        }
    }
}
