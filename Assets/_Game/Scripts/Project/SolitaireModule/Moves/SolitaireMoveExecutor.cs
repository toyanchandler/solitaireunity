using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireMoveExecutor
    {
        private readonly SolitaireMoveResolver _moveResolver;

        public SolitaireMoveExecutor(SolitaireMoveResolver moveResolver)
        {
            _moveResolver = moveResolver ?? throw new System.ArgumentNullException(nameof(moveResolver));
        }

        public bool TryExecute(
            SolitaireBoardState board,
            SolitaireMove move,
            bool allowFoundationToTableau,
            bool autoFlipTableauTopCard,
            SolitaireMoveHistory moveHistory,
            out SolitaireMoveResult result)
        {
            if (!SolitaireMoveHandlerRegistry.TryValidate(board, move, _moveResolver, allowFoundationToTableau, out string reason))
            {
                result = SolitaireMoveResult.Rejected(reason);
                return false;
            }

            SolitaireBoardSnapshot snapshot = moveHistory != null ? board.CreateSnapshot() : null;
            result = SolitaireMoveHandlerRegistry.Execute(board, move, autoFlipTableauTopCard);

            if (result.IsAccepted)
                moveHistory?.Push(snapshot);

            return result.IsAccepted;
        }
    }
}
