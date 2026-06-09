using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal delegate bool SolitaireSourceStartRule(
        SolitaireBoardState board,
        CardState movingCard,
        PileRef source,
        bool allowFoundationToTableau,
        out string reason);

    internal delegate bool SolitaireTargetAcceptRule(
        SolitaireBoardState board,
        CardState movingCard,
        SolitaireMove move,
        out string reason);

    internal delegate bool SolitaireCardInputRule(
        SolitaireBoardState board,
        SolitaireDeckConfigSO config,
        int cardId,
        CardState card,
        SolitaireMoveResolver moveResolver);

    internal static partial class SolitairePileMoveRules
    {
        public static bool TryCanStartFromSource(
            SolitaireBoardState board,
            CardState movingCard,
            PileRef source,
            bool allowFoundationToTableau,
            out string reason) =>
            SourceStart.TryEvaluate(board, movingCard, source, allowFoundationToTableau, out reason);

        public static bool TryCanMoveToTarget(
            SolitaireBoardState board,
            CardState movingCard,
            SolitaireMove move,
            out string reason) =>
            TargetAccept.TryEvaluate(board, movingCard, move, out reason);

        public static bool CanCardReceiveInput(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            int cardId,
            CardState card,
            SolitaireMoveResolver moveResolver) =>
            CardInput.CanReceive(board, config, cardId, card, moveResolver);

        internal static class Result
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
