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
            SolitaireHintLogic.InputValidation.RequireCollectInputs(board, config, target);
            return SolitaireHintLogic.Collect.GatherAll(board, config, _moveResolver, target);
        }

        public bool TryGetHint(SolitaireBoardState board, SolitaireDeckConfigSO config, int cycleIndex, out SolitaireHint hint)
        {
            int count = CollectHints(board, config, _hints);
            return SolitaireHintLogic.CycleIndex.TryResolveHintAtIndex(_hints, count, cycleIndex, out hint);
        }

        public bool TryGetNextAutoCompleteMove(SolitaireBoardState board, SolitaireDeckConfigSO config, out SolitaireHint hint) =>
            SolitaireHintLogic.AutoComplete.TryFindNext(board, config, _moveResolver, out hint);

        public bool TryGetNextFoundationMove(SolitaireBoardState board, SolitaireDeckConfigSO config, out SolitaireHint hint) =>
            SolitaireHintLogic.FoundationHints.TryFindFirst(board, config, _moveResolver, out hint);
    }
}
