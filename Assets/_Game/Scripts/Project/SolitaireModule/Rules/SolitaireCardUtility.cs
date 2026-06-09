using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    public static class SolitaireCardUtility
    {
        public const int CardCount = 52;
        public const int SuitCount = 4;
        public const int RankCount = 13;
        public const int FoundationCount = 4;
        public const int TableauCount = 7;

        public static int GetCardId(CardSuit suit, CardRank rank)
        {
            return ((int)suit * RankCount) + ((int)rank - 1);
        }

        public static CardSuit GetSuitFromId(int cardId)
        {
            return (CardSuit)(cardId / RankCount);
        }

        public static CardRank GetRankFromId(int cardId)
        {
            return (CardRank)((cardId % RankCount) + 1);
        }

        public static int GetFoundationIndex(CardSuit suit)
        {
            return (int)suit;
        }

        public static bool HasOppositeColor(CardState first, CardState second)
        {
            return first.Color != second.Color;
        }

        public static string GetRankLabel(CardRank rank)
        {
            return rank switch
            {
                CardRank.Ace => "A",
                CardRank.Jack => "J",
                CardRank.Queen => "Q",
                CardRank.King => "K",
                _ => ((int)rank).ToString()
            };
        }

        public static string GetSuitLabel(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Hearts => "H",
                CardSuit.Diamonds => "D",
                CardSuit.Clubs => "C",
                CardSuit.Spades => "S",
                _ => "?"
            };
        }
    }
}
