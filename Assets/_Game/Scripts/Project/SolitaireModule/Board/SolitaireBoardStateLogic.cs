using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static class SolitaireBoardStateLogic
    {
        internal static class CardAccess
        {
            public static bool IsValidCardId(int cardId, int cardCount) =>
                (uint)cardId < (uint)cardCount;

            public static void EnsureValidCardId(int cardId, int cardCount)
            {
                if (!IsValidCardId(cardId, cardCount))
                    throw new IndexOutOfRangeException(nameof(cardId));
            }
        }

        internal static class PileSetup
        {
            public static void InitializeFoundations(FixedCardPileState[] foundations)
            {
                for (int i = 0; i < foundations.Length; i++)
                    foundations[i] = new FixedCardPileState(SolitairePileType.Foundation, i);
            }

            public static void InitializeTableaus(FixedCardPileState[] tableaus)
            {
                for (int i = 0; i < tableaus.Length; i++)
                    tableaus[i] = new FixedCardPileState(SolitairePileType.Tableau, i);
            }
        }

        internal static class PileClearing
        {
            public static void ClearAll(
                FixedCardPileState stock,
                FixedCardPileState waste,
                FixedCardPileState[] foundations,
                FixedCardPileState[] tableaus)
            {
                stock.Clear();
                waste.Clear();
                ClearAllFoundations(foundations);
                ClearAllTableaus(tableaus);
            }

            public static void ClearAllFoundations(FixedCardPileState[] foundations)
            {
                for (int i = 0; i < foundations.Length; i++)
                    foundations[i].Clear();
            }

            public static void ClearAllTableaus(FixedCardPileState[] tableaus)
            {
                for (int i = 0; i < tableaus.Length; i++)
                    tableaus[i].Clear();
            }
        }

        internal static class CardInitialization
        {
            public static CardState CreateDefault(int cardId) =>
                new CardState
                {
                    Id = cardId,
                    Suit = SolitaireCardUtility.GetSuitFromId(cardId),
                    Rank = SolitaireCardUtility.GetRankFromId(cardId),
                    IsFaceUp = false,
                    CurrentPileType = SolitairePileType.Stock,
                    CurrentPileIndex = 0,
                    IndexInPile = -1
                };

            public static void InitializeAll(CardState[] cards)
            {
                for (int cardId = 0; cardId < cards.Length; cardId++)
                    cards[cardId] = CreateDefault(cardId);
            }
        }

        internal static class CardPlacement
        {
            public static void AssignToPile(ref CardState card, PileRef target, int indexInPile, bool isFaceUp)
            {
                card.IsFaceUp = isFaceUp;
                card.CurrentPileType = target.Type;
                card.CurrentPileIndex = target.Index;
                card.IndexInPile = indexInPile;
            }

            public static void SyncPileIndex(ref CardState card, PileRef pileRef, int indexInPile)
            {
                card.CurrentPileType = pileRef.Type;
                card.CurrentPileIndex = pileRef.Index;
                card.IndexInPile = indexInPile;
            }

            public static void RefreshAllIndicesInPile(
                CardState[] cards,
                FixedCardPileState pile,
                PileRef pileRef)
            {
                for (int i = 0; i < pile.Count; i++)
                {
                    ref CardState card = ref cards[pile[i]];
                    SyncPileIndex(ref card, pileRef, i);
                }
            }
        }

        internal static class DeckShuffle
        {
            public static int[] BuildShuffledDeck(int seed)
            {
                int[] deck = CreateSequentialDeck(SolitaireCardUtility.CardCount);
                ShuffleInPlace(deck, new Random(seed));
                return deck;
            }

            public static int[] CreateSequentialDeck(int cardCount)
            {
                int[] deck = new int[cardCount];

                for (int i = 0; i < deck.Length; i++)
                    deck[i] = i;

                return deck;
            }

            public static void ShuffleInPlace(int[] deck, Random random)
            {
                for (int i = deck.Length - 1; i > 0; i--)
                {
                    int swapIndex = random.Next(i + 1);
                    (deck[i], deck[swapIndex]) = (deck[swapIndex], deck[i]);
                }
            }
        }

        internal static class Deal
        {
            public static PileRef CreateTableauPileRef(int column) =>
                new PileRef(SolitairePileType.Tableau, column);

            public static PileRef StockPileRef => new PileRef(SolitairePileType.Stock, 0);

            public static bool IsTableauDealFaceUp(int row, int column) =>
                row == column;

            public static void DealTableaus(
                int[] deck,
                ref int cursor,
                Action<int, PileRef, bool> addCardToPile)
            {
                for (int column = 0; column < SolitaireCardUtility.TableauCount; column++)
                {
                    for (int row = 0; row <= column; row++)
                    {
                        int cardId = deck[cursor++];
                        bool isFaceUp = IsTableauDealFaceUp(row, column);
                        addCardToPile(cardId, CreateTableauPileRef(column), isFaceUp);
                    }
                }
            }

            public static void DealRemainingToStock(
                int[] deck,
                ref int cursor,
                Action<int, PileRef, bool> addCardToPile)
            {
                while (cursor < deck.Length)
                    addCardToPile(deck[cursor++], StockPileRef, false);
            }
        }

        internal static class UsedCardTracking
        {
            public static bool[] CreateMask(int cardCount) =>
                new bool[cardCount];

            public static void MarkPileCards(FixedCardPileState pile, bool[] used)
            {
                for (int i = 0; i < pile.Count; i++)
                    used[pile[i]] = true;
            }

            public static void MarkAllBoardPiles(
                FixedCardPileState stock,
                FixedCardPileState waste,
                FixedCardPileState[] foundations,
                FixedCardPileState[] tableaus,
                bool[] used)
            {
                MarkPileCards(stock, used);
                MarkPileCards(waste, used);
                MarkAllFoundations(foundations, used);
                MarkAllTableaus(tableaus, used);
            }

            public static void MarkAllFoundations(FixedCardPileState[] foundations, bool[] used)
            {
                for (int i = 0; i < foundations.Length; i++)
                    MarkPileCards(foundations[i], used);
            }

            public static void MarkAllTableaus(FixedCardPileState[] tableaus, bool[] used)
            {
                for (int i = 0; i < tableaus.Length; i++)
                    MarkPileCards(tableaus[i], used);
            }

            public static bool IsCardUnused(bool[] used, int cardId) =>
                !used[cardId];
        }

        internal static class WinCheck
        {
            public static int CountFoundationCards(FixedCardPileState[] foundations)
            {
                int count = 0;

                for (int i = 0; i < foundations.Length; i++)
                    count += foundations[i].Count;

                return count;
            }

            public static bool IsCompleteWin(int foundationCardCount) =>
                foundationCardCount == SolitaireCardUtility.CardCount;
        }

        internal static class Snapshot
        {
            public static bool HasMatchingShape(
                SolitaireBoardSnapshot snapshot,
                int cardCount,
                int foundationCount,
                int tableauCount) =>
                snapshot != null &&
                snapshot.Cards.Length == cardCount &&
                snapshot.Foundations.Length == foundationCount &&
                snapshot.Tableaus.Length == tableauCount;

            public static void EnsureNotNull(SolitaireBoardSnapshot snapshot)
            {
                if (snapshot == null)
                    throw new ArgumentNullException(nameof(snapshot));
            }

            public static void EnsureMatchingShape(
                SolitaireBoardSnapshot snapshot,
                int cardCount,
                int foundationCount,
                int tableauCount)
            {
                EnsureNotNull(snapshot);

                if (!HasMatchingShape(snapshot, cardCount, foundationCount, tableauCount))
                    throw new InvalidOperationException("Snapshot shape does not match this board.");
            }

            public static CardState[] CopyCards(CardState[] source)
            {
                var copy = new CardState[source.Length];

                for (int i = 0; i < source.Length; i++)
                    copy[i] = source[i];

                return copy;
            }

            public static SolitairePileSnapshot[] CreateFoundationSnapshots(FixedCardPileState[] foundations)
            {
                var snapshots = new SolitairePileSnapshot[foundations.Length];

                for (int i = 0; i < foundations.Length; i++)
                    snapshots[i] = SolitairePileSnapshot.Create(foundations[i]);

                return snapshots;
            }

            public static SolitairePileSnapshot[] CreateTableauSnapshots(FixedCardPileState[] tableaus)
            {
                var snapshots = new SolitairePileSnapshot[tableaus.Length];

                for (int i = 0; i < tableaus.Length; i++)
                    snapshots[i] = SolitairePileSnapshot.Create(tableaus[i]);

                return snapshots;
            }

            public static SolitaireBoardSnapshot Create(
                CardState[] cards,
                FixedCardPileState stock,
                FixedCardPileState waste,
                FixedCardPileState[] foundations,
                FixedCardPileState[] tableaus) =>
                new SolitaireBoardSnapshot(
                    CopyCards(cards),
                    SolitairePileSnapshot.Create(stock),
                    SolitairePileSnapshot.Create(waste),
                    CreateFoundationSnapshots(foundations),
                    CreateTableauSnapshots(tableaus));

            public static void Restore(
                SolitaireBoardSnapshot snapshot,
                CardState[] cards,
                FixedCardPileState stock,
                FixedCardPileState waste,
                FixedCardPileState[] foundations,
                FixedCardPileState[] tableaus)
            {
                EnsureMatchingShape(
                    snapshot,
                    cards.Length,
                    foundations.Length,
                    tableaus.Length);

                for (int i = 0; i < cards.Length; i++)
                    cards[i] = snapshot.Cards[i];

                snapshot.Stock.RestoreInto(stock);
                snapshot.Waste.RestoreInto(waste);

                for (int i = 0; i < foundations.Length; i++)
                    snapshot.Foundations[i].RestoreInto(foundations[i]);

                for (int i = 0; i < tableaus.Length; i++)
                    snapshot.Tableaus[i].RestoreInto(tableaus[i]);
            }
        }
    }
}
