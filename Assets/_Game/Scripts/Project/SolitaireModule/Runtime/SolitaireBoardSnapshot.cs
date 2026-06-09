using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireBoardSnapshot
    {
        public readonly CardState[] Cards;
        public readonly SolitairePileSnapshot Stock;
        public readonly SolitairePileSnapshot Waste;
        public readonly SolitairePileSnapshot[] Foundations;
        public readonly SolitairePileSnapshot[] Tableaus;

        public SolitaireBoardSnapshot(
            CardState[] cards,
            SolitairePileSnapshot stock,
            SolitairePileSnapshot waste,
            SolitairePileSnapshot[] foundations,
            SolitairePileSnapshot[] tableaus)
        {
            Cards = cards;
            Stock = stock;
            Waste = waste;
            Foundations = foundations;
            Tableaus = tableaus;
        }
    }

    public readonly struct SolitairePileSnapshot
    {
        private readonly int[] _cardIds;
        private readonly int _count;

        private SolitairePileSnapshot(int[] cardIds, int count)
        {
            _cardIds = cardIds;
            _count = count;
        }

        public static SolitairePileSnapshot Create(FixedCardPileState pile)
        {
            int[] cardIds = new int[SolitaireCardUtility.CardCount];
            pile.CopyAllTo(cardIds, out int count);
            return new SolitairePileSnapshot(cardIds, count);
        }

        public void RestoreInto(FixedCardPileState pile)
        {
            pile.RestoreFrom(_cardIds, _count);
        }
    }
}
