using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class StockHints
        {
            public static bool HasStockCards(SolitaireBoardState board) =>
                board.Stock.Count > 0;

            public static bool CanRecycleWaste(SolitaireDeckConfigSO config, SolitaireBoardState board) =>
                config.AllowWasteRecycle && board.Waste.Count > 0;

            public static bool TryCreateStockAction(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                out SolitaireHint hint)
            {
                if (TryCreateDrawFromStock(board, out hint))
                    return true;

                if (TryCreateRecycleWaste(config, board, out hint))
                    return true;

                return HintResults.Fail(out hint);
            }

            public static int AppendStockAction(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireHint[] target,
                int count)
            {
                if (!TryCreateStockAction(board, config, out SolitaireHint hint))
                    return count;

                HintCollection.AppendIfUnique(target, ref count, hint);
                return count;
            }

            private static bool TryCreateDrawFromStock(SolitaireBoardState board, out SolitaireHint hint)
            {
                if (!HasStockCards(board))
                    return HintResults.Fail(out hint);

                hint = new SolitaireHint(SolitaireHintKind.StockAction, MoveFactory.CreateStockToWaste());
                return true;
            }

            private static bool TryCreateRecycleWaste(
                SolitaireDeckConfigSO config,
                SolitaireBoardState board,
                out SolitaireHint hint)
            {
                if (!CanRecycleWaste(config, board))
                    return HintResults.Fail(out hint);

                hint = new SolitaireHint(SolitaireHintKind.StockAction, MoveFactory.CreateWasteRecycleToStock());
                return true;
            }
        }
    }
}
