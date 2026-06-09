using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireHintService
    {
        public const int MaxHints = 96;
        public const int MaxAutoCompleteMoves = SolitaireCardUtility.CardCount;

        private readonly SolitaireMoveResolver _moveResolver;
        private readonly SolitaireHint[] _hints = new SolitaireHint[MaxHints];

        public SolitaireHintService(SolitaireMoveResolver moveResolver)
        {
            _moveResolver = moveResolver ?? throw new ArgumentNullException(nameof(moveResolver));
        }

        public int CollectHints(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireHint[] target)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            int count = 0;
            count = AppendFoundationMoves(board, config, target, count);
            count = AppendTableauRevealMoves(board, config, target, count);
            count = AppendWasteToTableauMoves(board, config, target, count);
            count = AppendTableauToTableauMoves(board, config, target, count, false);
            count = AppendStockAction(board, config, target, count);
            return count;
        }

        public bool TryGetHint(SolitaireBoardState board, SolitaireDeckConfigSO config, int cycleIndex, out SolitaireHint hint)
        {
            int count = CollectHints(board, config, _hints);

            if (count <= 0)
            {
                hint = SolitaireHint.None;
                return false;
            }

            int index = cycleIndex % count;

            if (index < 0)
                index += count;

            hint = _hints[index];
            return true;
        }

        public bool TryGetNextFoundationMove(SolitaireBoardState board, SolitaireDeckConfigSO config, out SolitaireHint hint)
        {
            var singleHint = new SolitaireHint[1];
            int count = 0;

            if (TryAppendFoundationMoveFromPile(board, config, board.Waste, ref count, singleHint))
            {
                hint = singleHint[0];
                return true;
            }

            for (int i = 0; i < board.Tableaus.Length; i++)
            {
                count = 0;

                if (TryAppendFoundationMoveFromPile(board, config, board.Tableaus[i], ref count, singleHint))
                {
                    hint = singleHint[0];
                    return true;
                }
            }

            hint = SolitaireHint.None;
            return false;
        }

        private int AppendFoundationMoves(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireHint[] target, int count)
        {
            TryAppendFoundationMoveFromPile(board, config, board.Waste, ref count, target);

            for (int i = 0; i < board.Tableaus.Length; i++)
                TryAppendFoundationMoveFromPile(board, config, board.Tableaus[i], ref count, target);

            return count;
        }

        private bool TryAppendFoundationMoveFromPile(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            FixedCardPileState pile,
            ref int count,
            SolitaireHint[] target)
        {
            int cardId = pile.PeekTop();

            if (cardId < 0)
                return false;

            SolitaireMove move = _moveResolver.ResolveAutoFoundationMove(board, cardId);

            if (!CanExecute(board, config, move))
                return false;

            return AppendUnique(target, ref count, new SolitaireHint(SolitaireHintKind.MoveToFoundation, move));
        }

        private int AppendTableauRevealMoves(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireHint[] target, int count)
        {
            return AppendTableauToTableauMoves(board, config, target, count, true);
        }

        private int AppendWasteToTableauMoves(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireHint[] target, int count)
        {
            int cardId = board.Waste.PeekTop();

            if (cardId < 0)
                return count;

            var source = new PileRef(SolitairePileType.Waste, 0);

            for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
            {
                var targetPile = new PileRef(SolitairePileType.Tableau, targetIndex);
                var move = new SolitaireMove(SolitaireMoveType.WasteToTableau, cardId, source, targetPile);

                if (CanExecute(board, config, move))
                    AppendUnique(target, ref count, new SolitaireHint(SolitaireHintKind.WasteToTableau, move));
            }

            return count;
        }

        private int AppendTableauToTableauMoves(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            SolitaireHint[] target,
            int count,
            bool revealOnly)
        {
            for (int sourceIndex = 0; sourceIndex < board.Tableaus.Length; sourceIndex++)
            {
                FixedCardPileState sourcePile = board.Tableaus[sourceIndex];

                for (int cardIndex = 0; cardIndex < sourcePile.Count; cardIndex++)
                {
                    int cardId = sourcePile[cardIndex];
                    CardState card = board.GetCard(cardId);

                    if (!card.IsFaceUp)
                        continue;

                    bool revealsHiddenCard = WillRevealHiddenTableauCard(board, sourcePile, cardIndex);

                    if (revealOnly && !revealsHiddenCard)
                        continue;

                    if (!revealOnly && revealsHiddenCard)
                        continue;

                    var source = new PileRef(SolitairePileType.Tableau, sourceIndex);

                    for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
                    {
                        if (targetIndex == sourceIndex)
                            continue;

                        var targetPile = new PileRef(SolitairePileType.Tableau, targetIndex);
                        var move = new SolitaireMove(SolitaireMoveType.TableauToTableau, cardId, source, targetPile);

                        if (!CanExecute(board, config, move))
                            continue;

                        SolitaireHintKind kind = revealsHiddenCard
                            ? SolitaireHintKind.RevealTableauByMove
                            : SolitaireHintKind.TableauToTableau;
                        AppendUnique(target, ref count, new SolitaireHint(kind, move));
                    }
                }
            }

            return count;
        }

        private int AppendStockAction(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireHint[] target, int count)
        {
            if (board.Stock.Count > 0)
            {
                var move = new SolitaireMove(
                    SolitaireMoveType.StockToWaste,
                    -1,
                    new PileRef(SolitairePileType.Stock, 0),
                    new PileRef(SolitairePileType.Waste, 0));
                AppendUnique(target, ref count, new SolitaireHint(SolitaireHintKind.StockAction, move));
                return count;
            }

            if (config.AllowWasteRecycle && board.Waste.Count > 0)
            {
                var move = new SolitaireMove(
                    SolitaireMoveType.WasteRecycleToStock,
                    -1,
                    new PileRef(SolitairePileType.Waste, 0),
                    new PileRef(SolitairePileType.Stock, 0));
                AppendUnique(target, ref count, new SolitaireHint(SolitaireHintKind.StockAction, move));
            }

            return count;
        }

        private bool CanExecute(SolitaireBoardState board, SolitaireDeckConfigSO config, SolitaireMove move)
        {
            return _moveResolver.CanExecute(board, move, config.AllowFoundationToTableau, out _);
        }

        private static bool WillRevealHiddenTableauCard(SolitaireBoardState board, FixedCardPileState sourcePile, int movingCardIndex)
        {
            if (sourcePile.PileType != SolitairePileType.Tableau || movingCardIndex <= 0)
                return false;

            int revealedCandidateId = sourcePile[movingCardIndex - 1];
            return !board.GetCard(revealedCandidateId).IsFaceUp;
        }

        private static bool AppendUnique(SolitaireHint[] target, ref int count, SolitaireHint hint)
        {
            if (!hint.IsValid || count >= target.Length)
                return false;

            for (int i = 0; i < count; i++)
            {
                if (AreSameMove(target[i].Move, hint.Move))
                    return false;
            }

            target[count] = hint;
            count++;
            return true;
        }

        private static bool AreSameMove(SolitaireMove a, SolitaireMove b)
        {
            return a.Type == b.Type &&
                   a.StartCardId == b.StartCardId &&
                   a.Source.Type == b.Source.Type &&
                   a.Source.Index == b.Source.Index &&
                   a.Target.Type == b.Target.Type &&
                   a.Target.Index == b.Target.Index;
        }
    }
}
