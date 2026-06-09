using System;
using System.Diagnostics;
using System.Text;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireBenchmarkRunner
    {
        private const int DealIterations = 20000;
        private const int ValidationIterations = 250000;
        private const int StockUndoIterations = 20000;
        private const int SnapshotIterations = 20000;
        private const int ScoreDispatchIterations = 500000;
        private const int HintEnumerationIterations = 100000;
        private const int AutoCompleteIterations = 20000;

        [MenuItem("Tools/Solitaire/Run Benchmarks")]
        public static void RunBenchmarksMenu()
        {
            Debug.Log(RunBenchmarks());
        }

        public static string RunBenchmarks()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            BenchmarkResult deal = BenchmarkInitialDeals();
            BenchmarkResult validation = BenchmarkMoveValidation();
            BenchmarkResult stockUndo = BenchmarkStockDrawUndo();
            BenchmarkResult snapshot = BenchmarkSnapshotRestore();
            BenchmarkResult scoreDispatch = BenchmarkScoreEventDispatchAndUpdate();
            BenchmarkResult hintEnumeration = BenchmarkHintEnumeration();
            BenchmarkResult autoComplete = BenchmarkAutoCompleteFoundationSweep();

            var builder = new StringBuilder(512);
            builder.AppendLine("SOLITAIRE_BENCHMARK_RESULTS");
            Append(builder, deal);
            Append(builder, validation);
            Append(builder, stockUndo);
            Append(builder, snapshot);
            Append(builder, scoreDispatch);
            Append(builder, hintEnumeration);
            Append(builder, autoComplete);
            builder.Append("Unity=").Append(Application.unityVersion).AppendLine();
            builder.Append("Platform=").Append(Application.platform).AppendLine();
            builder.Append("Device=").Append(SystemInfo.processorType).Append(" / ").Append(SystemInfo.graphicsDeviceName);
            return builder.ToString();
        }

        private static BenchmarkResult BenchmarkInitialDeals()
        {
            var board = new SolitaireBoardState();

            for (int i = 0; i < 100; i++)
                board.ResetAndDeal(104729 + i);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < DealIterations; i++)
                board.ResetAndDeal(104729 + i);

            stopwatch.Stop();
            return BenchmarkResult.From("Initial deal", DealIterations, stopwatch.Elapsed);
        }

        private static BenchmarkResult BenchmarkMoveValidation()
        {
            var board = CreateTwoCardTableauBoard();
            var resolver = new SolitaireMoveResolver();
            int movingCardId = SolitaireCardUtility.GetCardId(CardSuit.Hearts, CardRank.Seven);
            SolitaireMove move = resolver.ResolveDragMove(board, movingCardId, new PileRef(SolitairePileType.Tableau, 0));

            for (int i = 0; i < 1000; i++)
                resolver.CanExecute(board, move, false, out _);

            Stopwatch stopwatch = Stopwatch.StartNew();
            int acceptedCount = 0;

            for (int i = 0; i < ValidationIterations; i++)
            {
                if (resolver.CanExecute(board, move, false, out _))
                    acceptedCount++;
            }

            stopwatch.Stop();

            if (acceptedCount != ValidationIterations)
                throw new InvalidOperationException("Move validation benchmark setup is invalid.");

            return BenchmarkResult.From("Tableau move validation", ValidationIterations, stopwatch.Elapsed);
        }

        private static BenchmarkResult BenchmarkStockDrawUndo()
        {
            var board = new SolitaireBoardState();
            board.ResetAndDeal(104729);
            var context = new SolitaireRuntimeContext(board, null);
            var resolver = new SolitaireMoveResolver();
            var service = new SolitaireMoveService(resolver, new SolitaireMoveExecutor(resolver));
            SolitaireDeckConfigSO config = ScriptableObject.CreateInstance<SolitaireDeckConfigSO>();

            try
            {
                for (int i = 0; i < 100; i++)
                    DrawAndUndo(service, context, config);

                Stopwatch stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < StockUndoIterations; i++)
                    DrawAndUndo(service, context, config);

                stopwatch.Stop();
                return BenchmarkResult.From("Stock draw plus undo", StockUndoIterations, stopwatch.Elapsed);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static BenchmarkResult BenchmarkSnapshotRestore()
        {
            var board = new SolitaireBoardState();
            board.ResetAndDeal(104729);
            SolitaireBoardSnapshot snapshot = board.CreateSnapshot();

            for (int i = 0; i < 100; i++)
                board.RestoreSnapshot(snapshot);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < SnapshotIterations; i++)
            {
                snapshot = board.CreateSnapshot();
                board.RestoreSnapshot(snapshot);
            }

            stopwatch.Stop();
            return BenchmarkResult.From("Snapshot create plus restore", SnapshotIterations, stopwatch.Elapsed);
        }

        private static BenchmarkResult BenchmarkScoreEventDispatchAndUpdate()
        {
            SolitaireScoreConfigSO config = ScriptableObject.CreateInstance<SolitaireScoreConfigSO>();
            int score = 0;

            void ApplyScoreAction(SolitaireScoreAction action)
            {
                score = config.ClampScore(score + config.GetDelta(action));
            }

            try
            {
                EventManager.SolitaireEvents.ScoreActionPerformed += ApplyScoreAction;
                SolitaireScoreAction[] actions =
                {
                    SolitaireScoreAction.MoveToFoundation,
                    SolitaireScoreAction.RevealTableauCard,
                    SolitaireScoreAction.Undo,
                    SolitaireScoreAction.StockDraw,
                    SolitaireScoreAction.StockRecycle,
                    SolitaireScoreAction.MoveToTableau
                };

                for (int i = 0; i < 1000; i++)
                    EventManager.SolitaireEvents.ScoreActionPerformed?.Invoke(actions[i % actions.Length]);

                Stopwatch stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < ScoreDispatchIterations; i++)
                    EventManager.SolitaireEvents.ScoreActionPerformed?.Invoke(actions[i % actions.Length]);

                stopwatch.Stop();

                if (score <= 0)
                    throw new InvalidOperationException("Score dispatch benchmark setup is invalid.");

                return BenchmarkResult.From("Score event dispatch plus update", ScoreDispatchIterations, stopwatch.Elapsed);
            }
            finally
            {
                EventManager.SolitaireEvents.ScoreActionPerformed -= ApplyScoreAction;
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static BenchmarkResult BenchmarkHintEnumeration()
        {
            var board = CreateHintRichBoard();
            SolitaireDeckConfigSO config = ScriptableObject.CreateInstance<SolitaireDeckConfigSO>();
            var service = new SolitaireHintService(new SolitaireMoveResolver());
            var hints = new SolitaireHint[SolitaireHintService.MaxHints];

            try
            {
                for (int i = 0; i < 1000; i++)
                    service.CollectHints(board, config, hints);

                Stopwatch stopwatch = Stopwatch.StartNew();
                int hintCount = 0;

                for (int i = 0; i < HintEnumerationIterations; i++)
                    hintCount += service.CollectHints(board, config, hints);

                stopwatch.Stop();

                if (hintCount <= 0)
                    throw new InvalidOperationException("Hint enumeration benchmark setup is invalid.");

                return BenchmarkResult.From("Hint enumeration", HintEnumerationIterations, stopwatch.Elapsed);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static BenchmarkResult BenchmarkAutoCompleteFoundationSweep()
        {
            SolitaireDeckConfigSO config = ScriptableObject.CreateInstance<SolitaireDeckConfigSO>();
            var resolver = new SolitaireMoveResolver();
            var service = new SolitaireMoveService(resolver, new SolitaireMoveExecutor(resolver));

            try
            {
                for (int i = 0; i < 100; i++)
                    RunAutoCompleteSweep(service, config);

                Stopwatch stopwatch = Stopwatch.StartNew();
                int movedCount = 0;

                for (int i = 0; i < AutoCompleteIterations; i++)
                    movedCount += RunAutoCompleteSweep(service, config);

                stopwatch.Stop();

                if (movedCount != AutoCompleteIterations * SolitaireCardUtility.FoundationCount)
                    throw new InvalidOperationException("AutoComplete benchmark setup is invalid.");

                return BenchmarkResult.From("AutoComplete foundation sweep", AutoCompleteIterations, stopwatch.Elapsed);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static SolitaireBoardState CreateTwoCardTableauBoard()
        {
            var board = new SolitaireBoardState();
            board.ClearForDebugSetup();
            board.InitializeCardsForDebugSetup();
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Clubs, CardRank.Eight),
                new PileRef(SolitairePileType.Tableau, 0),
                true);
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Hearts, CardRank.Seven),
                new PileRef(SolitairePileType.Tableau, 1),
                true);
            return board;
        }

        private static SolitaireBoardState CreateHintRichBoard()
        {
            var board = new SolitaireBoardState();
            board.ClearForDebugSetup();
            board.InitializeCardsForDebugSetup();
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Hearts, CardRank.Ace),
                new PileRef(SolitairePileType.Waste, 0),
                true);
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Clubs, CardRank.Ten),
                new PileRef(SolitairePileType.Tableau, 0),
                false);
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Hearts, CardRank.Seven),
                new PileRef(SolitairePileType.Tableau, 0),
                true);
            board.AddCardToPile(
                SolitaireCardUtility.GetCardId(CardSuit.Spades, CardRank.Eight),
                new PileRef(SolitairePileType.Tableau, 1),
                true);
            return board;
        }

        private static SolitaireBoardState CreateAutoCompleteBoard()
        {
            var board = new SolitaireBoardState();
            board.ClearForDebugSetup();
            board.InitializeCardsForDebugSetup();

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
            {
                board.AddCardToPile(
                    SolitaireCardUtility.GetCardId((CardSuit)i, CardRank.Ace),
                    new PileRef(SolitairePileType.Tableau, i),
                    true);
            }

            return board;
        }

        private static int RunAutoCompleteSweep(SolitaireMoveService service, SolitaireDeckConfigSO config)
        {
            var context = new SolitaireRuntimeContext(CreateAutoCompleteBoard(), null);
            int movedCount = 0;

            for (int i = 0; i < SolitaireHintService.MaxAutoCompleteMoves; i++)
            {
                if (!service.TryGetNextAutoCompleteMove(context.BoardState, config, out SolitaireHint hint))
                    break;

                if (!service.TryAutoMoveToFoundation(context, config, hint.Move.StartCardId, out _, out _))
                    break;

                movedCount++;
            }

            return movedCount;
        }

        private static void DrawAndUndo(
            SolitaireMoveService service,
            SolitaireRuntimeContext context,
            SolitaireDeckConfigSO config)
        {
            if (!service.TryDrawOrRecycleStock(context, config, out _))
                throw new InvalidOperationException("Stock draw benchmark setup is invalid.");

            if (!service.TryUndo(context))
                throw new InvalidOperationException("Stock undo benchmark setup is invalid.");
        }

        private static void Append(StringBuilder builder, BenchmarkResult result)
        {
            builder
                .Append(result.Name)
                .Append(": iterations=").Append(result.Iterations)
                .Append(", totalMs=").Append(result.TotalMs.ToString("0.###"))
                .Append(", avgUs=").Append(result.AvgUs.ToString("0.###"))
                .Append(", opsPerSecond=").Append(result.OpsPerSecond.ToString("0"))
                .AppendLine();
        }

        private readonly struct BenchmarkResult
        {
            private BenchmarkResult(string name, int iterations, TimeSpan elapsed)
            {
                Name = name;
                Iterations = iterations;
                TotalMs = elapsed.TotalMilliseconds;
                AvgUs = elapsed.TotalMilliseconds * 1000.0 / iterations;
                OpsPerSecond = iterations / Math.Max(elapsed.TotalSeconds, 0.000001);
            }

            public string Name { get; }
            public int Iterations { get; }
            public double TotalMs { get; }
            public double AvgUs { get; }
            public double OpsPerSecond { get; }

            public static BenchmarkResult From(string name, int iterations, TimeSpan elapsed)
            {
                return new BenchmarkResult(name, iterations, elapsed);
            }
        }
    }
}
