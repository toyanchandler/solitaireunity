using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class Collect
        {
            public static int GatherAll(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target)
            {
                int count = 0;
                count = FoundationHints.AppendAllFromBoard(board, config, resolver, target, count);
                count = TableauToTableauHints.AppendAllFromBoard(board, config, resolver, target, count, revealOnly: true);
                count = WasteToTableauHints.AppendAllFromBoard(board, config, resolver, target, count);
                count = TableauToTableauHints.AppendAllFromBoard(board, config, resolver, target, count, revealOnly: false);
                count = StockHints.AppendStockAction(board, config, target, count);
                return count;
            }
        }

        internal static class AutoComplete
        {
            private delegate bool TryFindNextMove(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint);

            private static readonly TryFindNextMove[] Finders =
            {
                FoundationHints.TryFindFirst,
                WasteToTableauHints.TryFindFirst,
                TableauToTableauHints.TryFindFirstReveal
            };

            public static bool TryFindNext(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint)
            {
                for (int i = 0; i < Finders.Length; i++)
                {
                    if (Finders[i](board, config, resolver, out hint))
                        return true;
                }

                hint = SolitaireHint.None;
                return false;
            }
        }
    }
}
