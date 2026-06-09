using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitaireMoveHandlerRegistry
    {
        internal static class Validation
        {
            internal static class Stock
            {
                public static bool Validate(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    SolitaireMoveResolver moveResolver,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    IsValidDraw(board, move)
                        ? ValidationResult.Accept(out reason)
                        : ValidationResult.Reject("Stock draw move is not valid.", out reason);

                public static bool IsValidDraw(SolitaireBoardState board, SolitaireMove move) =>
                    move.Source.Type == SolitairePileType.Stock &&
                    move.Target.Type == SolitairePileType.Waste &&
                    board.Stock.Count > 0;
            }

            internal static class WasteRecycle
            {
                public static bool Validate(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    SolitaireMoveResolver moveResolver,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    IsValidRecycle(board, move)
                        ? ValidationResult.Accept(out reason)
                        : ValidationResult.Reject("Waste recycle move is not valid.", out reason);

                public static bool IsValidRecycle(SolitaireBoardState board, SolitaireMove move) =>
                    move.Source.Type == SolitairePileType.Waste &&
                    move.Target.Type == SolitairePileType.Stock &&
                    board.Waste.Count > 0;
            }

            internal static class FlipTableau
            {
                public static bool Validate(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    SolitaireMoveResolver moveResolver,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    moveResolver.CanFlipTableauTop(board, move.Source, move.StartCardId, out reason);
            }

            internal static class CardTransfer
            {
                public static bool Validate(
                    SolitaireBoardState board,
                    SolitaireMove move,
                    SolitaireMoveResolver moveResolver,
                    bool allowFoundationToTableau,
                    out string reason) =>
                    moveResolver.CanExecute(board, move, allowFoundationToTableau, out reason);
            }
        }
    }
}
