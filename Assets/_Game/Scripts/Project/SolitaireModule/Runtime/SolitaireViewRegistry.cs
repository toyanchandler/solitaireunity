using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireViewRegistry
    {
        private readonly CardView[] _cards = new CardView[SolitaireCardUtility.CardCount];
        private readonly SolitaireSlotAnchor[] _stock = new SolitaireSlotAnchor[1];
        private readonly SolitaireSlotAnchor[] _waste = new SolitaireSlotAnchor[1];
        private readonly SolitaireSlotAnchor[] _foundations = new SolitaireSlotAnchor[SolitaireCardUtility.FoundationCount];
        private readonly SolitaireSlotAnchor[] _tableaus = new SolitaireSlotAnchor[SolitaireCardUtility.TableauCount];

        public void RegisterCard(CardView card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if ((uint)card.CardId >= (uint)_cards.Length)
                throw new ArgumentOutOfRangeException(nameof(card), $"{card.name} has invalid CardId {card.CardId}.");

            _cards[card.CardId] = card;
        }

        public void RegisterSlot(SolitaireSlotAnchor slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            GetSlotArray(slot.PileType)[slot.PileIndex] = slot;
        }

        public CardView GetCard(int cardId)
        {
            return _cards[cardId];
        }

        public SolitaireSlotAnchor GetSlot(PileRef pileRef)
        {
            return GetSlotArray(pileRef.Type)[pileRef.Index];
        }

        public CardView[] Cards => _cards;
        public SolitaireSlotAnchor Stock => _stock[0];
        public SolitaireSlotAnchor Waste => _waste[0];
        public SolitaireSlotAnchor[] Tableaus => _tableaus;
        public SolitaireSlotAnchor[] Foundations => _foundations;

        public bool Validate(out string error)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                {
                    error = $"Missing registered Card_{i:00}.";
                    return false;
                }
            }

            if (_stock[0] == null || _waste[0] == null)
            {
                error = "Missing StockSlot or WasteSlot.";
                return false;
            }

            for (int i = 0; i < _foundations.Length; i++)
            {
                if (_foundations[i] == null)
                {
                    error = $"Missing Foundation slot {i}.";
                    return false;
                }
            }

            for (int i = 0; i < _tableaus.Length; i++)
            {
                if (_tableaus[i] == null)
                {
                    error = $"Missing Tableau slot {i}.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private SolitaireSlotAnchor[] GetSlotArray(SolitairePileType pileType)
        {
            switch (pileType)
            {
                case SolitairePileType.Stock:
                    return _stock;
                case SolitairePileType.Waste:
                    return _waste;
                case SolitairePileType.Foundation:
                    return _foundations;
                case SolitairePileType.Tableau:
                    return _tableaus;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pileType), pileType, null);
            }
        }
    }
}
