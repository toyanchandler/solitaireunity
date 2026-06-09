using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class TableauToTableauHints
        {
            public readonly struct MoveCandidate
            {
                public MoveCandidate(int cardId, int sourceIndex, int cardIndex, bool revealsHiddenCard)
                {
                    CardId = cardId;
                    SourceIndex = sourceIndex;
                    CardIndex = cardIndex;
                    RevealsHiddenCard = revealsHiddenCard;
                }

                public int CardId { get; }
                public int SourceIndex { get; }
                public int CardIndex { get; }
                public bool RevealsHiddenCard { get; }
            }

            public static bool TryCreateForTarget(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                int cardId,
                int sourceIndex,
                int targetIndex,
                bool revealsHiddenCard,
                out SolitaireHint hint)
            {
                if (!TableauMoveFilter.IsDifferentColumn(sourceIndex, targetIndex))
                    return HintResults.Fail(out hint);

                SolitaireMove move = MoveFactory.CreateTableauToTableau(cardId, sourceIndex, targetIndex);

                if (!Execution.CanExecute(board, resolver, config, move))
                    return HintResults.Fail(out hint);

                SolitaireHintKind kind = HintKindResolution.ForTableauToTableau(revealsHiddenCard);
                hint = new SolitaireHint(kind, move);
                return true;
            }

            public static bool ShouldConsiderCard(
                SolitaireBoardState board,
                FixedCardPileState sourcePile,
                int cardIndex,
                bool revealOnly)
            {
                CardState card = board.GetCard(sourcePile[cardIndex]);

                if (!CardQueries.IsFaceUp(card))
                    return false;

                bool revealsHiddenCard = RevealDetection.WillRevealHiddenTableauCard(board, sourcePile, cardIndex);
                return TableauMoveFilter.ShouldInclude(revealOnly, revealsHiddenCard);
            }

            public static int AppendAllFromBoard(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count,
                bool revealOnly)
            {
                for (int sourceIndex = 0; sourceIndex < board.Tableaus.Length; sourceIndex++)
                {
                    count = AppendFromSourcePile(
                        board,
                        config,
                        resolver,
                        board.Tableaus[sourceIndex],
                        sourceIndex,
                        target,
                        count,
                        revealOnly);
                }

                return count;
            }

            public static bool TryFindFirstReveal(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                out SolitaireHint hint)
            {
                for (int sourceIndex = 0; sourceIndex < board.Tableaus.Length; sourceIndex++)
                {
                    if (TryFindRevealInSourcePile(
                        board,
                        config,
                        resolver,
                        board.Tableaus[sourceIndex],
                        sourceIndex,
                        out hint))
                    {
                        return true;
                    }
                }

                return HintResults.Fail(out hint);
            }

            private static int AppendFromSourcePile(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                FixedCardPileState sourcePile,
                int sourceIndex,
                SolitaireHint[] target,
                int count,
                bool revealOnly)
            {
                for (int cardIndex = 0; cardIndex < sourcePile.Count; cardIndex++)
                {
                    if (!TryResolveCandidate(board, sourcePile, sourceIndex, cardIndex, revealOnly, out MoveCandidate candidate))
                        continue;

                    count = AppendForCandidate(board, config, resolver, target, count, candidate);
                }

                return count;
            }

            private static int AppendForCandidate(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                SolitaireHint[] target,
                int count,
                MoveCandidate candidate)
            {
                for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
                {
                    if (TryCreateForTarget(
                        board,
                        config,
                        resolver,
                        candidate.CardId,
                        candidate.SourceIndex,
                        targetIndex,
                        candidate.RevealsHiddenCard,
                        out SolitaireHint hint))
                    {
                        HintCollection.AppendIfUnique(target, ref count, hint);
                    }
                }

                return count;
            }

            private static bool TryFindRevealInSourcePile(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                FixedCardPileState sourcePile,
                int sourceIndex,
                out SolitaireHint hint)
            {
                for (int cardIndex = 0; cardIndex < sourcePile.Count; cardIndex++)
                {
                    if (!TryResolveCandidate(board, sourcePile, sourceIndex, cardIndex, revealOnly: true, out MoveCandidate candidate))
                        continue;

                    if (TryFindTargetForCandidate(board, config, resolver, candidate, out hint))
                        return true;
                }

                return HintResults.Fail(out hint);
            }

            private static bool TryFindTargetForCandidate(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireMoveResolver resolver,
                MoveCandidate candidate,
                out SolitaireHint hint)
            {
                for (int targetIndex = 0; targetIndex < board.Tableaus.Length; targetIndex++)
                {
                    if (TryCreateForTarget(
                        board,
                        config,
                        resolver,
                        candidate.CardId,
                        candidate.SourceIndex,
                        targetIndex,
                        candidate.RevealsHiddenCard,
                        out hint))
                    {
                        return true;
                    }
                }

                return HintResults.Fail(out hint);
            }

            private static bool TryResolveCandidate(
                SolitaireBoardState board,
                FixedCardPileState sourcePile,
                int sourceIndex,
                int cardIndex,
                bool revealOnly,
                out MoveCandidate candidate)
            {
                if (!ShouldConsiderCard(board, sourcePile, cardIndex, revealOnly))
                {
                    candidate = default;
                    return false;
                }

                candidate = new MoveCandidate(
                    sourcePile[cardIndex],
                    sourceIndex,
                    cardIndex,
                    RevealDetection.WillRevealHiddenTableauCard(board, sourcePile, cardIndex));
                return true;
            }
        }
    }
}
