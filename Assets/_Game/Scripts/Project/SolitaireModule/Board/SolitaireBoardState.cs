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
            SolitaireBoardStateLogic.PileSetup.InitializeFoundations(_foundations);
            SolitaireBoardStateLogic.PileSetup.InitializeTableaus(_tableaus);
            _pileLookup = new SolitairePileTypeTable<FixedCardPileState>(_stock, _waste, _foundations, _tableaus);
        }

        public ref CardState GetCardRef(int cardId)
        {
            SolitaireBoardStateLogic.CardAccess.EnsureValidCardId(cardId, _cards.Length);
            return ref _cards[cardId];
        }

        public CardState GetCard(int cardId)
        {
            SolitaireBoardStateLogic.CardAccess.EnsureValidCardId(cardId, _cards.Length);
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

            int[] deck = SolitaireBoardStateLogic.DeckShuffle.BuildShuffledDeck(seed);
            int cursor = 0;

            SolitaireBoardStateLogic.Deal.DealTableaus(deck, ref cursor, AddCardToPile);
            SolitaireBoardStateLogic.Deal.DealRemainingToStock(deck, ref cursor, AddCardToPile);
        }

        public void ClearForDebugSetup() => ClearPiles();

        public void InitializeCardsForDebugSetup() => InitializeCards();

        public void ParkUnusedCardsInStock()
        {
            bool[] used = SolitaireBoardStateLogic.UsedCardTracking.CreateMask(SolitaireCardUtility.CardCount);
            SolitaireBoardStateLogic.UsedCardTracking.MarkAllBoardPiles(
                _stock,
                _waste,
                _foundations,
                _tableaus,
                used);

            var stockRef = SolitaireBoardStateLogic.Deal.StockPileRef;

            for (int cardId = 0; cardId < used.Length; cardId++)
            {
                if (!SolitaireBoardStateLogic.UsedCardTracking.IsCardUnused(used, cardId))
                    continue;

                AddCardToPile(cardId, stockRef, false);
            }
        }

        public void AddCardToPile(int cardId, PileRef target, bool isFaceUp)
        {
            FixedCardPileState pile = GetPile(target);
            pile.Add(cardId);

            ref CardState card = ref GetCardRef(cardId);
            SolitaireBoardStateLogic.CardPlacement.AssignToPile(ref card, target, pile.Count - 1, isFaceUp);
        }

        public void RefreshPileIndices(PileRef pileRef) =>
            SolitaireBoardStateLogic.CardPlacement.RefreshAllIndicesInPile(_cards, GetPile(pileRef), pileRef);

        public bool IsWon()
        {
            int foundationCardCount = SolitaireBoardStateLogic.WinCheck.CountFoundationCards(_foundations);
            return SolitaireBoardStateLogic.WinCheck.IsCompleteWin(foundationCardCount);
        }

        public SolitaireBoardSnapshot CreateSnapshot() =>
            SolitaireBoardStateLogic.Snapshot.Create(_cards, _stock, _waste, _foundations, _tableaus);

        public void RestoreSnapshot(SolitaireBoardSnapshot snapshot) =>
            SolitaireBoardStateLogic.Snapshot.Restore(snapshot, _cards, _stock, _waste, _foundations, _tableaus);

        private void ClearPiles() =>
            SolitaireBoardStateLogic.PileClearing.ClearAll(_stock, _waste, _foundations, _tableaus);

        private void InitializeCards() =>
            SolitaireBoardStateLogic.CardInitialization.InitializeAll(_cards);
    }
}
