using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireBoardState
    {
        private readonly CardState[] _cards = new CardState[SolitaireCardUtility.CardCount];
        private readonly FixedCardPileState _stock = new FixedCardPileState(SolitairePileType.Stock, 0);
        private readonly FixedCardPileState _waste = new FixedCardPileState(SolitairePileType.Waste, 0);
        private readonly FixedCardPileState[] _foundations = new FixedCardPileState[SolitaireCardUtility.FoundationCount];
        private readonly FixedCardPileState[] _tableaus = new FixedCardPileState[SolitaireCardUtility.TableauCount];
        private readonly SolitairePileTypeTable<FixedCardPileState> _pileLookup;

        public SolitaireBoardState()
        {
            for (int i = 0; i < _foundations.Length; i++)
                _foundations[i] = new FixedCardPileState(SolitairePileType.Foundation, i);

            for (int i = 0; i < _tableaus.Length; i++)
                _tableaus[i] = new FixedCardPileState(SolitairePileType.Tableau, i);

            _pileLookup = new SolitairePileTypeTable<FixedCardPileState>(_stock, _waste, _foundations, _tableaus);
        }

        public ref CardState GetCardRef(int cardId)
        {
            if ((uint)cardId >= (uint)_cards.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            return ref _cards[cardId];
        }

        public CardState GetCard(int cardId)
        {
            if ((uint)cardId >= (uint)_cards.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            return _cards[cardId];
        }

        public FixedCardPileState GetPile(PileRef pile) => _pileLookup.Resolve(pile);

        public FixedCardPileState Stock => _stock;
        public FixedCardPileState Waste => _waste;
        public FixedCardPileState[] Foundations => _foundations;
        public FixedCardPileState[] Tableaus => _tableaus;

        public void ResetAndDeal(int seed)
        {
            ClearPiles();
            InitializeCards();

            int[] deck = BuildShuffledDeck(seed);
            int cursor = 0;

            for (int column = 0; column < SolitaireCardUtility.TableauCount; column++)
            {
                for (int row = 0; row <= column; row++)
                {
                    int cardId = deck[cursor++];
                    bool isFaceUp = row == column;
                    AddCardToPile(cardId, new PileRef(SolitairePileType.Tableau, column), isFaceUp);
                }
            }

            while (cursor < deck.Length)
            {
                AddCardToPile(deck[cursor++], new PileRef(SolitairePileType.Stock, 0), false);
            }
        }

        public void ClearForDebugSetup()
        {
            ClearPiles();
        }

        public void InitializeCardsForDebugSetup()
        {
            InitializeCards();
        }

        public void ParkUnusedCardsInStock()
        {
            bool[] used = new bool[SolitaireCardUtility.CardCount];
            MarkPileCardsUsed(_stock, used);
            MarkPileCardsUsed(_waste, used);

            for (int i = 0; i < _foundations.Length; i++)
                MarkPileCardsUsed(_foundations[i], used);

            for (int i = 0; i < _tableaus.Length; i++)
                MarkPileCardsUsed(_tableaus[i], used);

            var stockRef = new PileRef(SolitairePileType.Stock, 0);

            for (int cardId = 0; cardId < used.Length; cardId++)
            {
                if (used[cardId])
                    continue;

                AddCardToPile(cardId, stockRef, false);
            }
        }

        private static void MarkPileCardsUsed(FixedCardPileState pile, bool[] used)
        {
            for (int i = 0; i < pile.Count; i++)
                used[pile[i]] = true;
        }

        public void AddCardToPile(int cardId, PileRef target, bool isFaceUp)
        {
            FixedCardPileState pile = GetPile(target);
            pile.Add(cardId);

            ref CardState card = ref GetCardRef(cardId);
            card.IsFaceUp = isFaceUp;
            card.CurrentPileType = target.Type;
            card.CurrentPileIndex = target.Index;
            card.IndexInPile = pile.Count - 1;
        }

        public void RefreshPileIndices(PileRef pileRef)
        {
            FixedCardPileState pile = GetPile(pileRef);

            for (int i = 0; i < pile.Count; i++)
            {
                ref CardState card = ref GetCardRef(pile[i]);
                card.CurrentPileType = pileRef.Type;
                card.CurrentPileIndex = pileRef.Index;
                card.IndexInPile = i;
            }
        }

        public bool IsWon()
        {
            int count = 0;

            for (int i = 0; i < _foundations.Length; i++)
                count += _foundations[i].Count;

            return count == SolitaireCardUtility.CardCount;
        }

        public SolitaireBoardSnapshot CreateSnapshot()
        {
            var cardSnapshot = new CardState[_cards.Length];

            for (int i = 0; i < _cards.Length; i++)
                cardSnapshot[i] = _cards[i];

            var foundationSnapshots = new SolitairePileSnapshot[_foundations.Length];
            var tableauSnapshots = new SolitairePileSnapshot[_tableaus.Length];

            for (int i = 0; i < _foundations.Length; i++)
                foundationSnapshots[i] = SolitairePileSnapshot.Create(_foundations[i]);

            for (int i = 0; i < _tableaus.Length; i++)
                tableauSnapshots[i] = SolitairePileSnapshot.Create(_tableaus[i]);

            return new SolitaireBoardSnapshot(
                cardSnapshot,
                SolitairePileSnapshot.Create(_stock),
                SolitairePileSnapshot.Create(_waste),
                foundationSnapshots,
                tableauSnapshots);
        }

        public void RestoreSnapshot(SolitaireBoardSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            if (snapshot.Cards.Length != _cards.Length ||
                snapshot.Foundations.Length != _foundations.Length ||
                snapshot.Tableaus.Length != _tableaus.Length)
            {
                throw new InvalidOperationException("Snapshot shape does not match this board.");
            }

            for (int i = 0; i < _cards.Length; i++)
                _cards[i] = snapshot.Cards[i];

            snapshot.Stock.RestoreInto(_stock);
            snapshot.Waste.RestoreInto(_waste);

            for (int i = 0; i < _foundations.Length; i++)
                snapshot.Foundations[i].RestoreInto(_foundations[i]);

            for (int i = 0; i < _tableaus.Length; i++)
                snapshot.Tableaus[i].RestoreInto(_tableaus[i]);
        }

        private void ClearPiles()
        {
            _stock.Clear();
            _waste.Clear();

            for (int i = 0; i < _foundations.Length; i++)
                _foundations[i].Clear();

            for (int i = 0; i < _tableaus.Length; i++)
                _tableaus[i].Clear();
        }

        private void InitializeCards()
        {
            for (int cardId = 0; cardId < _cards.Length; cardId++)
            {
                _cards[cardId] = new CardState
                {
                    Id = cardId,
                    Suit = SolitaireCardUtility.GetSuitFromId(cardId),
                    Rank = SolitaireCardUtility.GetRankFromId(cardId),
                    IsFaceUp = false,
                    CurrentPileType = SolitairePileType.Stock,
                    CurrentPileIndex = 0,
                    IndexInPile = -1
                };
            }
        }

        private static int[] BuildShuffledDeck(int seed)
        {
            int[] deck = new int[SolitaireCardUtility.CardCount];

            for (int i = 0; i < deck.Length; i++)
                deck[i] = i;

            var random = new Random(seed);

            for (int i = deck.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (deck[i], deck[swapIndex]) = (deck[swapIndex], deck[i]);
            }

            return deck;
        }
    }
}
