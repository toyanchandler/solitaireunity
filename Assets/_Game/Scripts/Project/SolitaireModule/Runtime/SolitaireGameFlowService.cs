using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireGameFlowService
    {
        private readonly int[] _foundationCounts = new int[SolitaireCardUtility.FoundationCount];

        public void ResetFoundationTracking(SolitaireBoardState board)
        {
            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                _foundationCounts[i] = board.Foundations[i].Count;
        }

        public void PrepareNewDeal(SolitaireRuntimeContext context, SolitaireDeckConfigSO config)
        {
            context.MoveHistory.Clear();
            context.SelectionState.Clear();

            int seed = config.UseFixedDealSeed
                ? config.DealSeed
                : UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            context.BoardState.ResetAndDeal(seed);
            ResetFoundationTracking(context.BoardState);
        }

        public void PrepareDebugScenario(SolitaireRuntimeContext context, SolitaireDebugScenarioId scenario)
        {
            if (scenario == SolitaireDebugScenarioId.None)
                throw new ArgumentException("Debug scenario cannot be None.", nameof(scenario));

            if (SolitaireDebugScenarioApplier.IsFlowScenario(scenario))
                throw new ArgumentException("Flow debug scenarios cannot be applied to board state.", nameof(scenario));

            context.MoveHistory.Clear();
            context.SelectionState.Clear();
            SolitaireDebugScenarioApplier.Apply(context.BoardState, scenario);
            ResetFoundationTracking(context.BoardState);
        }

        public bool IsWon(SolitaireBoardState board) => board.IsWon();

        public FoundationProgressDelta[] CollectFoundationProgressChanges(SolitaireBoardState board)
        {
            var changes = new FoundationProgressDelta[SolitaireCardUtility.FoundationCount];
            int changeCount = 0;

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
            {
                int currentCount = board.Foundations[i].Count;

                if (currentCount == _foundationCounts[i])
                    continue;

                _foundationCounts[i] = currentCount;
                changes[changeCount++] = new FoundationProgressDelta(i, currentCount);
            }

            if (changeCount == changes.Length)
                return changes;

            var trimmed = new FoundationProgressDelta[changeCount];
            Array.Copy(changes, trimmed, changeCount);
            return trimmed;
        }
    }

    public readonly struct FoundationProgressDelta
    {
        public int FoundationIndex { get; }
        public int CardCount { get; }

        public FoundationProgressDelta(int foundationIndex, int cardCount)
        {
            FoundationIndex = foundationIndex;
            CardCount = cardCount;
        }
    }
}
