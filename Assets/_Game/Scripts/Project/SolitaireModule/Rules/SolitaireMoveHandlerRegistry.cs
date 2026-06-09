using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal delegate bool SolitaireMoveValidationHandler(
        SolitaireBoardState board,
        SolitaireMove move,
        SolitaireMoveResolver moveResolver,
        bool allowFoundationToTableau,
        out string reason);

    internal delegate SolitaireMoveResult SolitaireMoveExecutionHandler(
        SolitaireBoardState board,
        SolitaireMove move,
        bool autoFlipTableauTopCard);

    internal static partial class SolitaireMoveHandlerRegistry
    {
        private static readonly SolitaireMoveValidationHandler[] ValidationHandlers =
            HandlerTables.CreateValidationHandlers();

        private static readonly SolitaireMoveExecutionHandler[] ExecutionHandlers =
            HandlerTables.CreateExecutionHandlers();

        public static bool TryValidate(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            if (!Guard.HasBoard(board))
                return ValidationResult.Reject("Board is missing.", out reason);

            if (!Dispatch.TryGetHandler(ValidationHandlers, move.Type, out SolitaireMoveValidationHandler handler))
                return ValidationResult.Reject("Unsupported move type.", out reason);

            return handler(board, move, moveResolver, allowFoundationToTableau, out reason);
        }

        public static SolitaireMoveResult Execute(
            SolitaireBoardState board,
            SolitaireMove move,
            bool autoFlipTableauTopCard) =>
            Dispatch.TryGetHandler(ExecutionHandlers, move.Type, out SolitaireMoveExecutionHandler handler)
                ? handler(board, move, autoFlipTableauTopCard)
                : SolitaireMoveResult.Rejected("Unsupported move type.");

        internal static class Guard
        {
            public static bool HasBoard(SolitaireBoardState board) => board != null;
        }

        internal static class ValidationResult
        {
            public static bool Accept(out string reason)
            {
                reason = string.Empty;
                return true;
            }

            public static bool Reject(string message, out string reason)
            {
                reason = message;
                return false;
            }
        }
    }
}
