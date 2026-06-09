using System;
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

    internal static class SolitaireMoveHandlerRegistry
    {
        private static readonly SolitaireMoveValidationHandler[] ValidationHandlers = CreateValidationHandlers();
        private static readonly SolitaireMoveExecutionHandler[] ExecutionHandlers = CreateExecutionHandlers();

        public static bool TryValidate(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            if (board == null)
                return Reject("Board is missing.", out reason);

            if (!TryGetHandler(ValidationHandlers, move.Type, out SolitaireMoveValidationHandler handler))
                return Reject("Unsupported move type.", out reason);

            return handler(board, move, moveResolver, allowFoundationToTableau, out reason);
        }

        public static SolitaireMoveResult Execute(
            SolitaireBoardState board,
            SolitaireMove move,
            bool autoFlipTableauTopCard)
        {
            return TryGetHandler(ExecutionHandlers, move.Type, out SolitaireMoveExecutionHandler handler)
                ? handler(board, move, autoFlipTableauTopCard)
                : SolitaireMoveResult.Rejected("Unsupported move type.");
        }

        private static SolitaireMoveValidationHandler[] CreateValidationHandlers()
        {
            var handlers = new SolitaireMoveValidationHandler[GetMoveTypeSlotCount()];
            handlers[(int)SolitaireMoveType.StockToWaste] = ValidateStockToWaste;
            handlers[(int)SolitaireMoveType.WasteRecycleToStock] = ValidateWasteRecycleToStock;
            handlers[(int)SolitaireMoveType.FlipTableauTop] = ValidateFlipTableauTop;
            handlers[(int)SolitaireMoveType.WasteToTableau] = ValidateCardTransfer;
            handlers[(int)SolitaireMoveType.WasteToFoundation] = ValidateCardTransfer;
            handlers[(int)SolitaireMoveType.TableauToTableau] = ValidateCardTransfer;
            handlers[(int)SolitaireMoveType.TableauToFoundation] = ValidateCardTransfer;
            handlers[(int)SolitaireMoveType.FoundationToTableau] = ValidateCardTransfer;
            handlers[(int)SolitaireMoveType.AutoMoveToFoundation] = ValidateCardTransfer;
            return handlers;
        }

        private static SolitaireMoveExecutionHandler[] CreateExecutionHandlers()
        {
            var handlers = new SolitaireMoveExecutionHandler[GetMoveTypeSlotCount()];
            handlers[(int)SolitaireMoveType.StockToWaste] = ExecuteStockToWaste;
            handlers[(int)SolitaireMoveType.WasteRecycleToStock] = ExecuteWasteRecycleToStock;
            handlers[(int)SolitaireMoveType.FlipTableauTop] = ExecuteFlipTableauTop;
            handlers[(int)SolitaireMoveType.WasteToTableau] = ExecuteCardTransfer;
            handlers[(int)SolitaireMoveType.WasteToFoundation] = ExecuteCardTransfer;
            handlers[(int)SolitaireMoveType.TableauToTableau] = ExecuteCardTransfer;
            handlers[(int)SolitaireMoveType.TableauToFoundation] = ExecuteCardTransfer;
            handlers[(int)SolitaireMoveType.FoundationToTableau] = ExecuteCardTransfer;
            handlers[(int)SolitaireMoveType.AutoMoveToFoundation] = ExecuteCardTransfer;
            return handlers;
        }

        private static bool TryGetHandler<THandler>(THandler[] handlers, SolitaireMoveType moveType, out THandler handler)
            where THandler : class
        {
            int index = (int)moveType;

            if ((uint)index >= (uint)handlers.Length)
            {
                handler = null;
                return false;
            }

            handler = handlers[index];
            return handler != null;
        }

        private static int GetMoveTypeSlotCount()
        {
            int max = 0;
            Array values = Enum.GetValues(typeof(SolitaireMoveType));

            for (int i = 0; i < values.Length; i++)
                max = Math.Max(max, (int)(SolitaireMoveType)values.GetValue(i));

            return max + 1;
        }

        private static bool ValidateStockToWaste(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            bool valid = move.Source.Type == SolitairePileType.Stock &&
                         move.Target.Type == SolitairePileType.Waste &&
                         board.Stock.Count > 0;
            return valid
                ? Accept(out reason)
                : Reject("Stock draw move is not valid.", out reason);
        }

        private static bool ValidateWasteRecycleToStock(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            bool valid = move.Source.Type == SolitairePileType.Waste &&
                         move.Target.Type == SolitairePileType.Stock &&
                         board.Waste.Count > 0;
            return valid
                ? Accept(out reason)
                : Reject("Waste recycle move is not valid.", out reason);
        }

        private static bool ValidateFlipTableauTop(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            return moveResolver.CanFlipTableauTop(board, move.Source, move.StartCardId, out reason);
        }

        private static bool ValidateCardTransfer(
            SolitaireBoardState board,
            SolitaireMove move,
            SolitaireMoveResolver moveResolver,
            bool allowFoundationToTableau,
            out string reason)
        {
            return moveResolver.CanExecute(board, move, allowFoundationToTableau, out reason);
        }

        private static SolitaireMoveResult ExecuteStockToWaste(SolitaireBoardState board, SolitaireMove move, bool autoFlipTableauTopCard)
        {
            int cardId = board.Stock.RemoveTop();
            return cardId < 0
                ? SolitaireMoveResult.Rejected("Stock is empty.")
                : AcceptAfterStockDraw(board, cardId);
        }

        private static SolitaireMoveResult AcceptAfterStockDraw(SolitaireBoardState board, int cardId)
        {
            board.AddCardToPile(cardId, new PileRef(SolitairePileType.Waste, 0), true);
            board.RefreshPileIndices(new PileRef(SolitairePileType.Stock, 0));
            return SolitaireMoveResult.Accepted();
        }

        private static SolitaireMoveResult ExecuteWasteRecycleToStock(SolitaireBoardState board, SolitaireMove move, bool autoFlipTableauTopCard)
        {
            return board.Waste.Count == 0
                ? SolitaireMoveResult.Rejected("Waste is empty.")
                : AcceptAfterWasteRecycle(board);
        }

        private static SolitaireMoveResult AcceptAfterWasteRecycle(SolitaireBoardState board)
        {
            while (board.Waste.Count > 0)
            {
                int cardId = board.Waste.RemoveTop();
                board.AddCardToPile(cardId, new PileRef(SolitairePileType.Stock, 0), false);
            }

            board.RefreshPileIndices(new PileRef(SolitairePileType.Waste, 0));
            return SolitaireMoveResult.Accepted();
        }

        private static SolitaireMoveResult ExecuteCardTransfer(SolitaireBoardState board, SolitaireMove move, bool autoFlipTableauTopCard)
        {
            FixedCardPileState source = board.GetPile(move.Source);
            int startIndex = source.IndexOf(move.StartCardId);

            if (startIndex < 0)
                return SolitaireMoveResult.Rejected("Start card is not in source pile.");

            int[] movingCards = new int[source.Count - startIndex];
            source.CopyRangeTo(startIndex, movingCards, out int movingCount);
            source.RemoveFromIndex(startIndex);

            for (int i = 0; i < movingCount; i++)
                board.AddCardToPile(movingCards[i], move.Target, true);

            board.RefreshPileIndices(move.Source);
            board.RefreshPileIndices(move.Target);

            int revealedCardId = autoFlipTableauTopCard && move.Source.Type == SolitairePileType.Tableau
                ? TryRevealTableauTop(board, move.Source)
                : -1;

            return SolitaireMoveResult.Accepted(revealedCardId);
        }

        private static int TryRevealTableauTop(SolitaireBoardState board, PileRef source)
        {
            FixedCardPileState sourceAfterMove = board.GetPile(source);
            int topCardId = sourceAfterMove.PeekTop();

            if (topCardId < 0)
                return -1;

            ref CardState topCard = ref board.GetCardRef(topCardId);

            if (topCard.IsFaceUp)
                return -1;

            topCard.IsFaceUp = true;
            return topCardId;
        }

        private static SolitaireMoveResult ExecuteFlipTableauTop(SolitaireBoardState board, SolitaireMove move, bool autoFlipTableauTopCard)
        {
            PileRef tableau = move.Source;

            if (tableau.Type != SolitairePileType.Tableau)
                return SolitaireMoveResult.Rejected("Only Tableau top card can flip.");

            int topCardId = board.GetPile(tableau).PeekTop();

            if (topCardId < 0)
                return SolitaireMoveResult.Rejected("Tableau is empty.");

            if (move.StartCardId >= 0 && topCardId != move.StartCardId)
                return SolitaireMoveResult.Rejected("Only top Tableau card can flip.");

            ref CardState card = ref board.GetCardRef(topCardId);
            card.IsFaceUp = true;
            return SolitaireMoveResult.Accepted(topCardId);
        }

        private static bool Accept(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        private static bool Reject(string message, out string reason)
        {
            reason = message;
            return false;
        }
    }
}
