using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace _Game.Tests.SolitaireModule.EditMode
{
    public sealed class SolitaireRulesRuntimeTests
    {
        [Test]
        public void ResetAndDeal_CreatesStandardDrawOneKlondikeLayout()
        {
            object board = New(BoardState);
            Call(board, "ResetAndDeal", 104729);

            Assert.AreEqual(24, Count(Prop(board, "Stock")));
            Assert.AreEqual(0, Count(Prop(board, "Waste")));

            Array tableaus = (Array)Prop(board, "Tableaus");
            for (int column = 0; column < 7; column++)
            {
                object tableau = tableaus.GetValue(column);
                Assert.AreEqual(column + 1, Count(tableau), $"Tableau {column} count");

                for (int index = 0; index < Count(tableau); index++)
                {
                    object card = Card(board, PileCard(tableau, index));
                    Assert.AreEqual(index == Count(tableau) - 1, Field<bool>(card, "IsFaceUp"), $"Tableau {column} card {index} face state");
                    Assert.AreEqual("Tableau", Field(card, "CurrentPileType").ToString());
                    Assert.AreEqual(column, Field<int>(card, "CurrentPileIndex"));
                    Assert.AreEqual(index, Field<int>(card, "IndexInPile"));
                }
            }

            AssertEveryCardAppearsExactlyOnce(board);
        }

        [Test]
        public void TableauMoves_UseDescendingOppositeColorAndKingForEmptyColumn()
        {
            object resolver = New(MoveResolver);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(
                    ("Clubs", "Eight", "Tableau", 0, true),
                    ("Hearts", "Seven", "Tableau", 1, true)),
                "Hearts",
                "Seven",
                Pile("Tableau", 0),
                true);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(
                    ("Hearts", "Eight", "Tableau", 0, true),
                    ("Diamonds", "Seven", "Tableau", 1, true)),
                "Diamonds",
                "Seven",
                Pile("Tableau", 0),
                false);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(("Hearts", "Seven", "Tableau", 1, true)),
                "Hearts",
                "Seven",
                Pile("Tableau", 2),
                false);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(("Spades", "King", "Tableau", 1, true)),
                "Spades",
                "King",
                Pile("Tableau", 2),
                true);
        }

        [Test]
        public void FoundationMoves_RequireAceThenSameSuitAscendingSingleTopCard()
        {
            object resolver = New(MoveResolver);

            AssertCanExecuteDrag(resolver, CreateBoardWithCards(("Hearts", "Ace", "Tableau", 0, true)), "Hearts", "Ace", Pile("Foundation", 0), true);
            AssertCanExecuteDrag(resolver, CreateBoardWithCards(("Hearts", "Two", "Tableau", 0, true)), "Hearts", "Two", Pile("Foundation", 0), false);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(
                    ("Hearts", "Ace", "Foundation", 0, true),
                    ("Hearts", "Two", "Tableau", 0, true)),
                "Hearts",
                "Two",
                Pile("Foundation", 0),
                true);

            AssertCanExecuteDrag(
                resolver,
                CreateBoardWithCards(
                    ("Hearts", "Ace", "Tableau", 0, true),
                    ("Spades", "King", "Tableau", 0, true)),
                "Hearts",
                "Ace",
                Pile("Foundation", 0),
                false);
        }

        [Test]
        public void WasteRules_AllowOnlyTopWasteCardToStartMove()
        {
            object resolver = New(MoveResolver);
            object board = CreateBoardWithCards(
                ("Hearts", "Ace", "Waste", 0, true),
                ("Clubs", "Two", "Waste", 0, true));

            Assert.IsFalse(CanStartMove(resolver, board, CardId("Hearts", "Ace")));
            Assert.IsTrue(CanStartMove(resolver, board, CardId("Clubs", "Two")));
        }

        [Test]
        public void StockDraw_PushesUndoSnapshotAndUndoRestoresPreviousBoard()
        {
            object board = New(BoardState);
            Call(board, "ResetAndDeal", 104729);
            object context = New(RuntimeContext, board, null);
            object service = CreateMoveService();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);

            bool performed = TryDrawOrRecycle(service, context, config, out object actionResult);

            Assert.IsTrue(performed);
            Assert.IsFalse(Prop<bool>(actionResult, "WasRecycle"));
            Assert.AreEqual(23, Count(Prop(board, "Stock")));
            Assert.AreEqual(1, Count(Prop(board, "Waste")));
            Assert.IsTrue(Field<bool>(Card(board, PeekTop(Prop(board, "Waste"))), "IsFaceUp"));
            Assert.AreEqual(1, Prop<int>(Prop(context, "MoveHistory"), "Count"));

            Assert.IsTrue((bool)Call(service, "TryUndo", context));
            Assert.AreEqual(24, Count(Prop(board, "Stock")));
            Assert.AreEqual(0, Count(Prop(board, "Waste")));
            Assert.AreEqual(0, Prop<int>(Prop(context, "MoveHistory"), "Count"));

            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void WasteRecycle_ReturnsWasteToFaceDownStockWhenConfigured()
        {
            object board = New(BoardState);
            Call(board, "ResetAndDeal", 104729);
            object context = New(RuntimeContext, board, null);
            object service = CreateMoveService();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);

            while (Count(Prop(board, "Stock")) > 0)
                Assert.IsTrue(TryDrawOrRecycle(service, context, config, out _));

            Assert.AreEqual(0, Count(Prop(board, "Stock")));
            Assert.AreEqual(24, Count(Prop(board, "Waste")));

            Assert.IsTrue(TryDrawOrRecycle(service, context, config, out object actionResult));
            Assert.IsTrue(Prop<bool>(actionResult, "WasRecycle"));
            Assert.AreEqual(24, Count(Prop(board, "Stock")));
            Assert.AreEqual(0, Count(Prop(board, "Waste")));

            object stock = Prop(board, "Stock");
            for (int i = 0; i < Count(stock); i++)
                Assert.IsFalse(Field<bool>(Card(board, PileCard(stock, i)), "IsFaceUp"));

            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void Undo_AfterTableauMoveRestoresAutoRevealedCardAndPiles()
        {
            object resolver = New(MoveResolver);
            object executor = New(MoveExecutor, resolver);
            object history = New(MoveHistory);
            int hiddenCardId = CardId("Clubs", "Ten");
            int movingCardId = CardId("Hearts", "Seven");
            int targetCardId = CardId("Spades", "Eight");
            object board = CreateBoardWithCards(
                ("Clubs", "Ten", "Tableau", 0, false),
                ("Hearts", "Seven", "Tableau", 0, true),
                ("Spades", "Eight", "Tableau", 1, true));
            object move = Call(resolver, "ResolveDragMove", board, movingCardId, Pile("Tableau", 1));

            Assert.IsTrue(TryExecute(executor, board, move, false, true, history, out object result));
            Assert.AreEqual(hiddenCardId, Field<int>(result, "RevealedCardId"));
            Assert.IsTrue(Field<bool>(Card(board, hiddenCardId), "IsFaceUp"));
            Assert.AreEqual(1, Count(Tableau(board, 0)));
            Assert.AreEqual(2, Count(Tableau(board, 1)));

            Assert.IsTrue((bool)Call(history, "TryUndo", board));
            Assert.IsFalse(Field<bool>(Card(board, hiddenCardId), "IsFaceUp"));
            Assert.AreEqual(2, Count(Tableau(board, 0)));
            Assert.AreEqual(1, Count(Tableau(board, 1)));
            Assert.AreEqual(targetCardId, PeekTop(Tableau(board, 1)));
        }

        [Test]
        public void InvalidMoves_DoNotMutateBoardState()
        {
            object resolver = New(MoveResolver);
            object executor = New(MoveExecutor, resolver);
            int movingCardId = CardId("Diamonds", "Seven");
            object board = CreateBoardWithCards(
                ("Hearts", "Eight", "Tableau", 0, true),
                ("Diamonds", "Seven", "Tableau", 1, true));
            string before = DescribeBoard(board);
            object move = Call(resolver, "ResolveDragMove", board, movingCardId, Pile("Tableau", 0));

            Assert.IsFalse(TryExecute(executor, board, move, false, true, null, out object result));
            Assert.IsFalse(Field<bool>(result, "IsAccepted"));
            Assert.AreEqual(before, DescribeBoard(board));
        }

        [Test]
        public void BoardState_IsWonWhenAllCardsAreOnFoundations()
        {
            object board = New(BoardState);
            Call(board, "ClearForDebugSetup");
            Call(board, "InitializeCardsForDebugSetup");

            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };

            for (int suit = 0; suit < suits.Length; suit++)
            {
                for (int rank = 0; rank < ranks.Length; rank++)
                    AddCard(board, suits[suit], ranks[rank], "Foundation", suit, true);
            }

            Assert.IsTrue((bool)Call(board, "IsWon"));
        }

        [Test]
        public void SolitaireModule_KeepsReferenceReadmeLayerStructure()
        {
            string modulePath = GetProjectPath("Assets/_Game/Scripts/Project/SolitaireModule");

            AssertDirectoryExists(modulePath, "Data");
            AssertDirectoryExists(modulePath, "Rules");
            AssertDirectoryExists(modulePath, "Runtime");
            AssertDirectoryExists(modulePath, "Input");
            AssertDirectoryExists(modulePath, "Presentation");
            AssertDirectoryExists(modulePath, "Views");
            AssertDirectoryExists(modulePath, "Controllers");
            AssertDirectoryExists(modulePath, "Editor");
        }

        [Test]
        public void SolitaireModule_DoesNotUseFormerlySerializedAs()
        {
            string modulePath = GetProjectPath("Assets/_Game/Scripts/Project/SolitaireModule");
            foreach (string file in Directory.GetFiles(modulePath, "*.cs", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                Assert.IsFalse(text.Contains("FormerlySerializedAs"), file);
                Assert.IsFalse(text.Contains("UnityEngine.Serialization"), file);
            }
        }

        [Test]
        public void SolitaireScoreConfig_UsesGddDefaultMappingAndMinimumClamp()
        {
            ScriptableObject config = ScriptableObject.CreateInstance(ScoreConfig);

            try
            {
                Assert.AreEqual(10, GetScoreDelta(config, "MoveToFoundation"));
                Assert.AreEqual(5, GetScoreDelta(config, "RevealTableauCard"));
                Assert.AreEqual(-1, GetScoreDelta(config, "Undo"));
                Assert.AreEqual(0, GetScoreDelta(config, "StockDraw"));
                Assert.AreEqual(0, GetScoreDelta(config, "StockRecycle"));
                Assert.AreEqual(0, GetScoreDelta(config, "MoveToTableau"));
                Assert.AreEqual(0, Call(config, "ClampScore", -10));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SolitaireEvents_ExposeScoreActionSignalAndResetClearsIt()
        {
            Type eventsType = RequiredType("_Game.Scripts.Managers.Core.EventManager+SolitaireEvents");
            Type scoreActionType = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitaireScoreAction");
            FieldInfo field = eventsType.GetField("ScoreActionPerformed");

            Assert.NotNull(field);
            Assert.IsTrue(field.FieldType.IsGenericType);
            Assert.AreEqual(typeof(UnityEngine.Events.UnityAction<>), field.FieldType.GetGenericTypeDefinition());
            Assert.AreEqual(scoreActionType, field.FieldType.GetGenericArguments()[0]);

            eventsType.GetMethod("Reset").Invoke(null, Array.Empty<object>());
            Assert.IsNull(field.GetValue(null));
        }

        [Test]
        public void SolitaireHintService_PrioritizesFoundationBeforeRevealAndStock()
        {
            object service = New(HintService, New(MoveResolver));
            object board = CreateHintRichBoard();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);
            Array hints = Array.CreateInstance(Hint, 16);

            try
            {
                int count = (int)Call(service, "CollectHints", board, config, hints);

                Assert.GreaterOrEqual(count, 3);

                object first = hints.GetValue(0);
                Assert.AreEqual("MoveToFoundation", Field(first, "Kind").ToString());

                object move = Field(first, "Move");
                Assert.AreEqual(CardId("Hearts", "Ace"), Field<int>(move, "StartCardId"));
                Assert.AreEqual("Foundation", Field(Field(move, "Target"), "Type").ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SolitaireHintService_CyclesAcrossCollectedHints()
        {
            object service = New(HintService, New(MoveResolver));
            object board = CreateHintRichBoard();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);

            try
            {
                Assert.IsTrue(TryGetHint(service, board, config, 0, out object firstHint));
                Assert.IsTrue(TryGetHint(service, board, config, 1, out object secondHint));
                Assert.IsTrue(TryGetHint(service, board, config, 100, out object wrappedHint));

                Assert.IsTrue((bool)Prop(firstHint, "IsValid"));
                Assert.IsTrue((bool)Prop(secondHint, "IsValid"));
                Assert.IsTrue((bool)Prop(wrappedHint, "IsValid"));
                Assert.AreNotEqual(Field(firstHint, "Kind").ToString(), Field(secondHint, "Kind").ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void AutoCompleteFoundationMove_UsesMoveServiceAndPreservesUndoHistory()
        {
            object board = CreateAutoCompleteAcesBoard();
            object context = New(RuntimeContext, board, null);
            object service = CreateMoveService();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);

            try
            {
                int movedCount = 0;

                while (TryGetNextAutoCompleteMove(service, board, config, out object hint))
                {
                    object move = Field(hint, "Move");
                    int startCardId = Field<int>(move, "StartCardId");

                    Assert.IsTrue(TryAutoMoveToFoundation(service, context, config, startCardId));
                    movedCount++;
                }

                Assert.AreEqual(4, movedCount);

                Array foundations = (Array)Prop(board, "Foundations");
                for (int i = 0; i < foundations.Length; i++)
                    Assert.AreEqual(1, Count(foundations.GetValue(i)), $"Foundation {i}");

                Assert.AreEqual(4, Prop<int>(Prop(context, "MoveHistory"), "Count"));
                Assert.IsTrue((bool)Call(service, "TryUndo", context));
                Assert.AreEqual(3, Prop<int>(Prop(context, "MoveHistory"), "Count"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void AutoComplete_IncludesWasteToTableau_WhenNoFoundationMoveExists()
        {
            object board = CreateBoardWithCards(
                ("Spades", "Nine", "Waste", 0, true),
                ("Hearts", "Ten", "Tableau", 0, true));
            object service = CreateMoveService();
            ScriptableObject config = ScriptableObject.CreateInstance(Config);

            try
            {
                Assert.IsTrue(TryGetNextAutoCompleteMove(service, board, config, out object hint));
                Assert.AreEqual("WasteToTableau", Field(hint, "Kind").ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static object CreateMoveService()
        {
            object resolver = New(MoveResolver);
            return New(MoveService, resolver, New(MoveExecutor, resolver));
        }

        private static int GetScoreDelta(ScriptableObject config, string actionName)
        {
            return (int)Call(config, "GetDelta", EnumValue(ScoreAction, actionName));
        }

        private static bool TryDrawOrRecycle(object service, object context, ScriptableObject config, out object actionResult)
        {
            object[] args = { context, config, null };
            bool accepted = (bool)Invoke(service, "TryDrawOrRecycleStock", args);
            actionResult = args[2];
            return accepted;
        }

        private static bool TryGetHint(object service, object board, ScriptableObject config, int cycleIndex, out object hint)
        {
            object[] args = { board, config, cycleIndex, null };
            bool found = (bool)Invoke(service, "TryGetHint", args);
            hint = args[3];
            return found;
        }

        private static bool TryGetNextAutoCompleteMove(object service, object board, ScriptableObject config, out object hint)
        {
            object[] args = { board, config, null };
            bool found = (bool)Invoke(service, "TryGetNextAutoCompleteMove", args);
            hint = args[2];
            return found;
        }

        private static bool TryAutoMoveToFoundation(object service, object context, ScriptableObject config, int cardId)
        {
            object[] args = { context, config, cardId, null, null };
            return (bool)Invoke(service, "TryAutoMoveToFoundation", args);
        }

        private static bool TryExecute(object executor, object board, object move, bool allowFoundationToTableau, bool autoFlip, object history, out object result)
        {
            object[] args = { board, move, allowFoundationToTableau, autoFlip, history, null };
            bool accepted = (bool)Invoke(executor, "TryExecute", args);
            result = args[5];
            return accepted;
        }

        private static bool CanStartMove(object resolver, object board, int cardId)
        {
            object[] args = { board, cardId, false, null };
            return (bool)Invoke(resolver, "CanStartMove", args);
        }

        private static void AssertCanExecuteDrag(object resolver, object board, string suit, string rank, object target, bool expected)
        {
            int cardId = CardId(suit, rank);
            object move = Call(resolver, "ResolveDragMove", board, cardId, target);
            object[] args = { board, move, false, null };
            bool actual = (bool)Invoke(resolver, "CanExecute", args);
            Assert.AreEqual(expected, actual);
        }

        private static object CreateBoardWithCards(params (string suit, string rank, string pileType, int pileIndex, bool faceUp)[] cards)
        {
            object board = New(BoardState);
            Call(board, "ClearForDebugSetup");
            Call(board, "InitializeCardsForDebugSetup");

            for (int i = 0; i < cards.Length; i++)
                AddCard(board, cards[i].suit, cards[i].rank, cards[i].pileType, cards[i].pileIndex, cards[i].faceUp);

            return board;
        }

        private static object CreateHintRichBoard()
        {
            return CreateBoardWithCards(
                ("Hearts", "Ace", "Waste", 0, true),
                ("Clubs", "Ten", "Tableau", 0, false),
                ("Hearts", "Seven", "Tableau", 0, true),
                ("Spades", "Eight", "Tableau", 1, true),
                ("Spades", "King", "Tableau", 2, true),
                ("Diamonds", "Queen", "Tableau", 3, true));
        }

        private static object CreateAutoCompleteAcesBoard()
        {
            object board = New(BoardState);
            Call(board, "ClearForDebugSetup");
            Call(board, "InitializeCardsForDebugSetup");

            AddCard(board, "Hearts", "Ace", "Tableau", 0, true);
            AddCard(board, "Diamonds", "Ace", "Tableau", 1, true);
            AddCard(board, "Clubs", "Ace", "Tableau", 2, true);
            AddCard(board, "Spades", "Ace", "Tableau", 3, true);
            return board;
        }

        private static void AddCard(object board, string suit, string rank, string pileType, int pileIndex, bool faceUp)
        {
            Call(board, "AddCardToPile", CardId(suit, rank), Pile(pileType, pileIndex), faceUp);
        }

        private static int CardId(string suit, string rank)
        {
            return (int)StaticCall(CardUtility, "GetCardId", EnumValue(CardSuit, suit), EnumValue(CardRank, rank));
        }

        private static object Pile(string type, int index)
        {
            return New(PileRef, EnumValue(PileType, type), index);
        }

        private static object Card(object board, int cardId)
        {
            return Call(board, "GetCard", cardId);
        }

        private static object Tableau(object board, int index)
        {
            return ((Array)Prop(board, "Tableaus")).GetValue(index);
        }

        private static int Count(object pile)
        {
            return Prop<int>(pile, "Count");
        }

        private static int PeekTop(object pile)
        {
            return (int)Call(pile, "PeekTop");
        }

        private static int PileCard(object pile, int index)
        {
            return (int)pile.GetType().GetProperty("Item").GetValue(pile, new object[] { index });
        }

        private static void AssertEveryCardAppearsExactlyOnce(object board)
        {
            var seen = new bool[52];

            MarkPile(Prop(board, "Stock"), seen);
            MarkPile(Prop(board, "Waste"), seen);

            Array foundations = (Array)Prop(board, "Foundations");
            for (int i = 0; i < foundations.Length; i++)
                MarkPile(foundations.GetValue(i), seen);

            Array tableaus = (Array)Prop(board, "Tableaus");
            for (int i = 0; i < tableaus.Length; i++)
                MarkPile(tableaus.GetValue(i), seen);

            for (int cardId = 0; cardId < seen.Length; cardId++)
                Assert.IsTrue(seen[cardId], $"Card {cardId:00} was not placed.");
        }

        private static void MarkPile(object pile, IList<bool> seen)
        {
            for (int i = 0; i < Count(pile); i++)
            {
                int cardId = PileCard(pile, i);
                Assert.IsFalse(seen[cardId], $"Card {cardId:00} appears more than once.");
                seen[cardId] = true;
            }
        }

        private static string DescribeBoard(object board)
        {
            var builder = new StringBuilder();

            AppendPile(builder, "S", Prop(board, "Stock"));
            AppendPile(builder, "W", Prop(board, "Waste"));

            Array foundations = (Array)Prop(board, "Foundations");
            for (int i = 0; i < foundations.Length; i++)
                AppendPile(builder, "F" + i, foundations.GetValue(i));

            Array tableaus = (Array)Prop(board, "Tableaus");
            for (int i = 0; i < tableaus.Length; i++)
                AppendPile(builder, "T" + i, tableaus.GetValue(i));

            for (int i = 0; i < 52; i++)
            {
                object card = Card(board, i);
                builder.Append("C").Append(i).Append(':')
                    .Append(Field(card, "CurrentPileType")).Append(',')
                    .Append(Field<int>(card, "CurrentPileIndex")).Append(',')
                    .Append(Field<int>(card, "IndexInPile")).Append(',')
                    .Append(Field<bool>(card, "IsFaceUp") ? '1' : '0').Append(';');
            }

            return builder.ToString();
        }

        private static void AppendPile(StringBuilder builder, string label, object pile)
        {
            builder.Append(label).Append('=');
            for (int i = 0; i < Count(pile); i++)
                builder.Append(PileCard(pile, i)).Append(',');
            builder.Append('|');
        }

        private static object New(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        private static object Call(object target, string methodName, params object[] args)
        {
            return Invoke(target, methodName, args);
        }

        private static object Invoke(object target, string methodName, object[] args)
        {
            return target.GetType().GetMethod(methodName).Invoke(target, args);
        }

        private static object StaticCall(Type type, string methodName, params object[] args)
        {
            return type.GetMethod(methodName).Invoke(null, args);
        }

        private static object Prop(object target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName).GetValue(target);
        }

        private static T Prop<T>(object target, string propertyName)
        {
            return (T)Prop(target, propertyName);
        }

        private static object Field(object target, string fieldName)
        {
            return target.GetType().GetField(fieldName).GetValue(target);
        }

        private static T Field<T>(object target, string fieldName)
        {
            return (T)Field(target, fieldName);
        }

        private static object EnumValue(Type enumType, string name)
        {
            return Enum.Parse(enumType, name);
        }

        private static Type RequiredType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp");
            Assert.NotNull(type, fullName);
            return type;
        }

        private static string GetProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void AssertDirectoryExists(string root, string child)
        {
            Assert.IsTrue(Directory.Exists(Path.Combine(root, child)), child);
        }

        private static readonly Type CardSuit = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.CardSuit");
        private static readonly Type CardRank = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.CardRank");
        private static readonly Type PileType = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitairePileType");
        private static readonly Type PileRef = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.PileRef");
        private static readonly Type CardUtility = RequiredType("_Game.Scripts.Project.SolitaireModule.Rules.SolitaireCardUtility");
        private static readonly Type BoardState = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireBoardState");
        private static readonly Type RuntimeContext = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireRuntimeContext");
        private static readonly Type MoveHistory = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireMoveHistory");
        private static readonly Type MoveResolver = RequiredType("_Game.Scripts.Project.SolitaireModule.Rules.SolitaireMoveResolver");
        private static readonly Type MoveExecutor = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireMoveExecutor");
        private static readonly Type MoveService = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireMoveService");
        private static readonly Type HintService = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireHintService");
        private static readonly Type Hint = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitaireHint");
        private static readonly Type Config = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitaireDeckConfigSO");
        private static readonly Type ScoreAction = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitaireScoreAction");
        private static readonly Type ScoreConfig = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitaireScoreConfigSO");
    }
}
