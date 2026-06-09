using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal delegate bool SolitaireSourceStartRule(
        SolitaireBoardState board,
        CardState movingCard,
        PileRef source,
        bool allowFoundationToTableau,
        out string reason);

    internal delegate bool SolitaireTargetAcceptRule(
        SolitaireBoardState board,
        CardState movingCard,
        SolitaireMove move,
        out string reason);

    internal delegate bool SolitaireCardInputRule(
        SolitaireBoardState board,
        SolitaireDeckConfigSO config,
        int cardId,
        CardState card,
        SolitaireMoveResolver moveResolver);

    internal static class SolitairePileMoveRules
    {
        private static readonly SolitaireSourceStartRule[] SourceStartRules = CreateSourceStartRules();
        private static readonly SolitaireTargetAcceptRule[] TargetAcceptRules = CreateTargetAcceptRules();
        private static readonly SolitaireCardInputRule[] CardInputRules = CreateCardInputRules();

        public static bool TryCanStartFromSource(
            SolitaireBoardState board,
            CardState movingCard,
            PileRef source,
            bool allowFoundationToTableau,
            out string reason)
        {
            SolitaireSourceStartRule rule = SourceStartRules[(int)source.Type];
            return rule != null
                ? rule(board, movingCard, source, allowFoundationToTableau, out reason)
                : Reject("Unsupported source pile.", out reason);
        }

        public static bool TryCanMoveToTarget(
            SolitaireBoardState board,
            CardState movingCard,
            SolitaireMove move,
            out string reason)
        {
            SolitaireTargetAcceptRule rule = TargetAcceptRules[(int)move.Target.Type];
            return rule != null
                ? rule(board, movingCard, move, out reason)
                : Reject("Unsupported target pile.", out reason);
        }

        public static bool CanCardReceiveInput(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            int cardId,
            CardState card,
            SolitaireMoveResolver moveResolver)
        {
            SolitaireCardInputRule rule = CardInputRules[(int)card.CurrentPileType];
            return rule != null && rule(board, config, cardId, card, moveResolver);
        }

        private static SolitaireSourceStartRule[] CreateSourceStartRules()
        {
            var rules = new SolitaireSourceStartRule[4];
            rules[(int)SolitairePileType.Waste] = CanStartFromWaste;
            rules[(int)SolitairePileType.Foundation] = CanStartFromFoundation;
            rules[(int)SolitairePileType.Tableau] = CanStartFromTableau;
            return rules;
        }

        private static SolitaireTargetAcceptRule[] CreateTargetAcceptRules()
        {
            var rules = new SolitaireTargetAcceptRule[4];
            rules[(int)SolitairePileType.Tableau] = CanMoveToTableau;
            rules[(int)SolitairePileType.Foundation] = CanMoveToFoundation;
            return rules;
        }

        private static SolitaireCardInputRule[] CreateCardInputRules()
        {
            var rules = new SolitaireCardInputRule[4];
            rules[(int)SolitairePileType.Stock] = CanReceiveStockInput;
            rules[(int)SolitairePileType.Tableau] = CanReceiveTableauInput;
            rules[(int)SolitairePileType.Waste] = CanReceiveDefaultMovableInput;
            rules[(int)SolitairePileType.Foundation] = CanReceiveDefaultMovableInput;
            return rules;
        }

        private static bool CanStartFromWaste(
            SolitaireBoardState board,
            CardState movingCard,
            PileRef source,
            bool allowFoundationToTableau,
            out string reason)
        {
            FixedCardPileState sourcePile = board.GetPile(source);
            return sourcePile.IsTopCard(movingCard.Id)
                ? Accept(out reason)
                : Reject("Only top Waste card can move.", out reason);
        }

        private static bool CanStartFromFoundation(
            SolitaireBoardState board,
            CardState movingCard,
            PileRef source,
            bool allowFoundationToTableau,
            out string reason)
        {
            if (!allowFoundationToTableau)
                return Reject("Foundation drag back is disabled.", out reason);

            FixedCardPileState sourcePile = board.GetPile(source);
            return sourcePile.IsTopCard(movingCard.Id)
                ? Accept(out reason)
                : Reject("Only top Foundation card can move.", out reason);
        }

        private static bool CanStartFromTableau(
            SolitaireBoardState board,
            CardState movingCard,
            PileRef source,
            bool allowFoundationToTableau,
            out string reason)
        {
            FixedCardPileState sourcePile = board.GetPile(source);
            int startIndex = sourcePile.IndexOf(movingCard.Id);

            if (startIndex < 0)
                return Reject("Card is not in source Tableau.", out reason);

            for (int i = startIndex; i < sourcePile.Count; i++)
            {
                CardState current = board.GetCard(sourcePile[i]);

                if (!current.IsFaceUp)
                    return Reject("Dragged Tableau sequence contains a face-down card.", out reason);

                if (i <= startIndex)
                    continue;

                CardState previous = board.GetCard(sourcePile[i - 1]);

                if (!IsValidTableauBuild(previous, current))
                    return Reject("Dragged Tableau sequence is not internally legal.", out reason);
            }

            return Accept(out reason);
        }

        private static bool CanMoveToTableau(
            SolitaireBoardState board,
            CardState movingCard,
            SolitaireMove move,
            out string reason)
        {
            FixedCardPileState targetPile = board.GetPile(move.Target);

            if (targetPile.Count == 0)
                return movingCard.Rank == CardRank.King
                    ? Accept(out reason)
                    : Reject("Only King can move to empty Tableau.", out reason);

            CardState targetTop = board.GetCard(targetPile.PeekTop());

            if (!targetTop.IsFaceUp)
                return Reject("Target Tableau top card is face-down.", out reason);

            return IsValidTableauBuild(targetTop, movingCard)
                ? Accept(out reason)
                : Reject("Tableau target requires descending rank with opposite color.", out reason);
        }

        private static bool CanMoveToFoundation(
            SolitaireBoardState board,
            CardState movingCard,
            SolitaireMove move,
            out string reason)
        {
            if (!board.GetPile(move.Source).IsTopCard(movingCard.Id))
                return Reject("Foundation accepts single top cards only.", out reason);

            FixedCardPileState foundation = board.GetPile(move.Target);

            if (foundation.Count == 0)
                return movingCard.Rank == CardRank.Ace
                    ? Accept(out reason)
                    : Reject("Foundation starts with Ace.", out reason);

            CardState top = board.GetCard(foundation.PeekTop());
            bool isLegal = top.Suit == movingCard.Suit && (int)movingCard.Rank == (int)top.Rank + 1;
            return isLegal
                ? Accept(out reason)
                : Reject("Foundation requires same suit ascending rank.", out reason);
        }

        private static bool CanReceiveStockInput(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            int cardId,
            CardState card,
            SolitaireMoveResolver moveResolver)
        {
            FixedCardPileState stockPile = board.GetPile(new PileRef(SolitairePileType.Stock, 0));
            return stockPile.IsTopCard(cardId);
        }

        private static bool CanReceiveTableauInput(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            int cardId,
            CardState card,
            SolitaireMoveResolver moveResolver)
        {
            FixedCardPileState tableauPile = board.GetPile(new PileRef(SolitairePileType.Tableau, card.CurrentPileIndex));
            return tableauPile.IsTopCard(cardId) ||
                   moveResolver.CanStartMove(board, cardId, config.AllowFoundationToTableau, out _);
        }

        private static bool CanReceiveDefaultMovableInput(
            SolitaireBoardState board,
            SolitaireDeckConfigSO config,
            int cardId,
            CardState card,
            SolitaireMoveResolver moveResolver)
        {
            return moveResolver.CanStartMove(board, cardId, config.AllowFoundationToTableau, out _);
        }

        private static bool IsValidTableauBuild(CardState upper, CardState lower)
        {
            bool isDescending = (int)lower.Rank == (int)upper.Rank - 1;
            bool isOppositeColor = SolitaireCardUtility.HasOppositeColor(upper, lower);
            return isDescending && isOppositeColor;
        }

        private static bool Accept(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        private static bool Reject(string message, out string reason)
        {
            reason = message;
            return false;
        }
    }
}
