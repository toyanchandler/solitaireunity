using System;
using System.Collections.Generic;
using _Game.Scripts.Project.SolitaireModule.Rules;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Data
{
    [CreateAssetMenu(fileName = "SolitaireCardVisualCatalog", menuName = "ThisGame/Solitaire/Card Visual Catalog", order = 0)]
    public sealed class SolitaireCardVisualCatalogSO : ScriptableObject
    {
        [SerializeField] private Sprite defaultBackSprite;
        [SerializeField] private CardSpriteDefinition[] cards = Array.Empty<CardSpriteDefinition>();

        public Sprite DefaultBackSprite => defaultBackSprite;
        public IReadOnlyList<CardSpriteDefinition> Cards => cards;

        public Sprite GetFrontSprite(CardSuit suit, CardRank rank)
        {
            return TryGetDefinition(suit, rank, out CardSpriteDefinition definition)
                ? definition.FrontSprite
                : null;
        }

        public Sprite GetBackSprite(CardSuit suit, CardRank rank)
        {
            return TryGetDefinition(suit, rank, out CardSpriteDefinition definition)
                ? definition.BackSprite
                : defaultBackSprite;
        }

        public bool TryGetDefinition(CardSuit suit, CardRank rank, out CardSpriteDefinition definition)
        {
            int cardId = SolitaireCardUtility.GetCardId(suit, rank);

            if (cards != null && cardId >= 0 && cardId < cards.Length)
            {
                CardSpriteDefinition candidate = cards[cardId];
                if (candidate.Suit == suit && candidate.Rank == rank)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = default;
            return false;
        }

        public bool ValidateComplete(out string error)
        {
            if (defaultBackSprite == null)
            {
                error = $"{name} is missing DefaultBackSprite.";
                return false;
            }

            if (cards == null || cards.Length != SolitaireCardUtility.CardCount)
            {
                error = $"{name} requires exactly {SolitaireCardUtility.CardCount} card sprite definitions.";
                return false;
            }

            for (int suitIndex = 0; suitIndex < SolitaireCardUtility.SuitCount; suitIndex++)
            {
                for (int rankIndex = 1; rankIndex <= SolitaireCardUtility.RankCount; rankIndex++)
                {
                    var suit = (CardSuit)suitIndex;
                    var rank = (CardRank)rankIndex;

                    if (!TryGetDefinition(suit, rank, out CardSpriteDefinition definition))
                    {
                        error = $"{name} is missing definition for {rank} of {suit}.";
                        return false;
                    }

                    if (definition.FrontSprite == null)
                    {
                        error = $"{name} is missing front sprite for {rank} of {suit}.";
                        return false;
                    }

                    if (definition.BackSprite == null)
                    {
                        error = $"{name} is missing back sprite for {rank} of {suit}.";
                        return false;
                    }
                }
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public struct CardSpriteDefinition
    {
        [SerializeField] private CardSuit suit;
        [SerializeField] private CardRank rank;
        [SerializeField] private Sprite frontSprite;
        [SerializeField] private Sprite backSprite;

        public CardSuit Suit => suit;
        public CardRank Rank => rank;
        public Sprite FrontSprite => frontSprite;
        public Sprite BackSprite => backSprite;
    }
}
