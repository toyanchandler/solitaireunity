using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireMoveService
    {
        private readonly SolitaireMoveResolver _moveResolver;
        private readonly SolitaireMoveExecutor _moveExecutor;
        private readonly SolitaireHintService _hintService;

        public SolitaireMoveService(SolitaireMoveResolver moveResolver, SolitaireMoveExecutor moveExecutor)
        {
            _moveResolver = moveResolver ?? throw new ArgumentNullException(nameof(moveResolver));
            _moveExecutor = moveExecutor ?? throw new ArgumentNullException(nameof(moveExecutor));
            _hintService = new SolitaireHintService(_moveResolver);
        }

        public bool CanMoveCardToSlot(SolitaireBoardState board, SolitaireDeckConfigSO config, int cardId, PileRef target)
        {
            SolitaireMove move = _moveResolver.ResolveDragMove(board, cardId, target);
            return _moveResolver.CanExecute(board, move, config.AllowFoundationToTableau, out _);
        }

        public bool CanStartDrag(SolitaireBoardState board, SolitaireDeckConfigSO config, int cardId)
        {
            return _moveResolver.CanStartMove(board, cardId, config.AllowFoundationToTableau, out _);
        }

        public bool CanCardReceiveInput(SolitaireBoardState board, SolitaireDeckConfigSO config, int cardId)
        {
            CardState card = board.GetCard(cardId);
            return SolitairePileMoveRules.CanCardReceiveInput(board, config, cardId, card, _moveResolver);
        }

        public bool TryMoveCardToSlot(
            SolitaireRuntimeContext context,
            SolitaireDeckConfigSO config,
            int cardId,
            PileRef target,
            out SolitaireMove move,
            out SolitaireMoveResult result)
        {
            move = _moveResolver.ResolveDragMove(context.BoardState, cardId, target);

            if (!_moveExecutor.TryExecute(
                    context.BoardState,
                    move,
                    config.AllowFoundationToTableau,
                    config.AutoFlipTableauTopCard,
                    context.MoveHistory,
                    out result))
            {
                return false;
            }

            return result.IsAccepted;
        }

        public bool TryAutoMoveToFoundation(
            SolitaireRuntimeContext context,
            SolitaireDeckConfigSO config,
            int cardId,
            out SolitaireMove move,
            out SolitaireMoveResult result)
        {
            move = _moveResolver.ResolveAutoFoundationMove(context.BoardState, cardId);

            if (!_moveExecutor.TryExecute(
                    context.BoardState,
                    move,
                    config.AllowFoundationToTableau,
                    config.AutoFlipTableauTopCard,
                    context.MoveHistory,
                    out result))
            {
                return false;
            }

            return result.IsAccepted;
        }

        public bool TryFlipTableauTop(
            SolitaireRuntimeContext context,
            SolitaireDeckConfigSO config,
            int cardId,
            out SolitaireMove move,
            out SolitaireMoveResult result)
        {
            CardState card = context.BoardState.GetCard(cardId);

            if (card.CurrentPileType != SolitairePileType.Tableau)
            {
                move = default;
                result = SolitaireMoveResult.Rejected("Only Tableau cards can flip.");
                return false;
            }

            FixedCardPileState pile = context.BoardState.GetPile(new PileRef(SolitairePileType.Tableau, card.CurrentPileIndex));

            if (!pile.IsTopCard(cardId) || card.IsFaceUp)
            {
                move = default;
                result = SolitaireMoveResult.Rejected("Only face-down Tableau top can flip.");
                return false;
            }

            move = new SolitaireMove(
                SolitaireMoveType.FlipTableauTop,
                cardId,
                new PileRef(SolitairePileType.Tableau, card.CurrentPileIndex),
                new PileRef(SolitairePileType.Tableau, card.CurrentPileIndex));

            if (!_moveExecutor.TryExecute(
                    context.BoardState,
                    move,
                    config.AllowFoundationToTableau,
                    config.AutoFlipTableauTopCard,
                    context.MoveHistory,
                    out result))
            {
                return false;
            }

            return result.IsAccepted;
        }

        public bool TryUndo(SolitaireRuntimeContext context)
        {
            return context.MoveHistory.TryUndo(context.BoardState);
        }

        public bool TryGetHint(SolitaireBoardState board, SolitaireDeckConfigSO config, int cycleIndex, out SolitaireHint hint)
        {
            return _hintService.TryGetHint(board, config, cycleIndex, out hint);
        }

        public bool TryGetNextAutoCompleteMove(SolitaireBoardState board, SolitaireDeckConfigSO config, out SolitaireHint hint)
        {
            return _hintService.TryGetNextAutoCompleteMove(board, config, out hint);
        }

        public bool TryDrawOrRecycleStock(
            SolitaireRuntimeContext context,
            SolitaireDeckConfigSO config,
            out SolitaireStockActionResult actionResult)
        {
            actionResult = default;

            if (!_moveResolver.CanDrawFromStock(context.BoardState))
                return false;

            if (context.BoardState.Stock.Count > 0)
            {
                actionResult = TryDrawConfiguredStockCards(context, config);
                return actionResult.WasPerformed;
            }

            if (!config.AllowWasteRecycle)
                return false;

            actionResult = TryRecycleWasteToStock(context, config);
            return actionResult.WasPerformed;
        }

        private SolitaireStockActionResult TryDrawConfiguredStockCards(SolitaireRuntimeContext context, SolitaireDeckConfigSO config)
        {
            SolitaireBoardSnapshot snapshot = context.BoardState.CreateSnapshot();
            int drawCount = Math.Max(1, (int)config.DrawMode);
            bool accepted = false;
            int lastDrawnCardId = -1;

            for (int i = 0; i < drawCount && context.BoardState.Stock.Count > 0; i++)
            {
                var move = new SolitaireMove(
                    SolitaireMoveType.StockToWaste,
                    -1,
                    new PileRef(SolitairePileType.Stock, 0),
                    new PileRef(SolitairePileType.Waste, 0));

                if (!_moveExecutor.TryExecute(
                        context.BoardState,
                        move,
                        config.AllowFoundationToTableau,
                        config.AutoFlipTableauTopCard,
                        null,
                        out SolitaireMoveResult result) || !result.IsAccepted)
                {
                    continue;
                }

                accepted = true;
                lastDrawnCardId = context.BoardState.Waste.PeekTop();
            }

            if (!accepted)
                return default;

            context.MoveHistory.Push(snapshot);
            return new SolitaireStockActionResult(true, false, lastDrawnCardId);
        }

        private SolitaireStockActionResult TryRecycleWasteToStock(SolitaireRuntimeContext context, SolitaireDeckConfigSO config)
        {
            var move = new SolitaireMove(
                SolitaireMoveType.WasteRecycleToStock,
                -1,
                new PileRef(SolitairePileType.Waste, 0),
                new PileRef(SolitairePileType.Stock, 0));

            if (!_moveExecutor.TryExecute(
                    context.BoardState,
                    move,
                    config.AllowFoundationToTableau,
                    config.AutoFlipTableauTopCard,
                    context.MoveHistory,
                    out SolitaireMoveResult result) || !result.IsAccepted)
            {
                return default;
            }

            return new SolitaireStockActionResult(true, true, -1);
        }

    }

    public readonly struct SolitaireStockActionResult
    {
        public bool WasPerformed { get; }
        public bool WasRecycle { get; }
        public int DrawnCardId { get; }

        public SolitaireStockActionResult(bool wasPerformed, bool wasRecycle, int drawnCardId)
        {
            WasPerformed = wasPerformed;
            WasRecycle = wasRecycle;
            DrawnCardId = drawnCardId;
        }
    }
}
