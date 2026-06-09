using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitairePileMoveRules
    {
        internal static class CardInput
        {
            private static readonly SolitaireCardInputRule[] Rules = CreateRules();

            public static bool CanReceive(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                int cardId,
                CardState card,
                SolitaireMoveResolver moveResolver)
            {
                SolitaireCardInputRule rule = Rules[(int)card.CurrentPileType];
                return rule != null && rule(board, config, cardId, card, moveResolver);
            }

            private static SolitaireCardInputRule[] CreateRules()
            {
                var rules = new SolitaireCardInputRule[4];
                rules[(int)SolitairePileType.Stock] = Stock.CanReceive;
                rules[(int)SolitairePileType.Tableau] = Tableau.CanReceive;
                rules[(int)SolitairePileType.Waste] = Movable.CanReceive;
                rules[(int)SolitairePileType.Foundation] = Movable.CanReceive;
                return rules;
            }

            internal static class Stock
            {
                public static bool CanReceive(
                    SolitaireBoardState board,
                    SolitaireDeckConfigSO config,
                    int cardId,
                    CardState card,
                    SolitaireMoveResolver moveResolver)
                {
                    FixedCardPileState stockPile = board.GetPile(new PileRef(SolitairePileType.Stock, 0));
                    return PileTop.IsTopCard(stockPile, cardId);
                }
            }

            internal static class Tableau
            {
                public static bool CanReceive(
                    SolitaireBoardState board,
                    SolitaireDeckConfigSO config,
                    int cardId,
                    CardState card,
                    SolitaireMoveResolver moveResolver) =>
                    IsTopCardOfColumn(board, card, cardId) ||
                    CanStartMoveFromSequence(board, config, cardId, moveResolver);

                public static bool IsTopCardOfColumn(SolitaireBoardState board, CardState card, int cardId)
                {
                    FixedCardPileState tableauPile = board.GetPile(
                        new PileRef(SolitairePileType.Tableau, card.CurrentPileIndex));
                    return PileTop.IsTopCard(tableauPile, cardId);
                }

                public static bool CanStartMoveFromSequence(
                    SolitaireBoardState board,
                    SolitaireDeckConfigSO config,
                    int cardId,
                    SolitaireMoveResolver moveResolver) =>
                    moveResolver.CanStartMove(board, cardId, config.AllowFoundationToTableau, out _);
            }

            internal static class Movable
            {
                public static bool CanReceive(
                    SolitaireBoardState board,
                    SolitaireDeckConfigSO config,
                    int cardId,
                    CardState card,
                    SolitaireMoveResolver moveResolver) =>
                    Tableau.CanStartMoveFromSequence(board, config, cardId, moveResolver);
            }
        }
    }
}
