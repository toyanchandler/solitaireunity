using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class CardRuntimeIdentity : MonoBehaviour
    {
        [SerializeField] private int cardId = -1;
        [SerializeField] private CardSuit suit;
        [SerializeField] private CardRank rank;

        public int CardId => cardId;
        public CardSuit Suit => suit;
        public CardRank Rank => rank;

        public void SetIdentity(int newCardId)
        {
            cardId = newCardId;
            suit = SolitaireCardUtility.GetSuitFromId(newCardId);
            rank = SolitaireCardUtility.GetRankFromId(newCardId);
            gameObject.name = $"Card_{newCardId:00}";
        }
    }
}
