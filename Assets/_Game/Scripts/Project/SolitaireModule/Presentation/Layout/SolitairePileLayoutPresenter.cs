using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitairePileLayoutPresenter
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private Action<float> _lockInputForAnimation;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            Action<float> lockInputForAnimation)
        {
            _config = config;
            _context = context;
            _lockInputForAnimation = lockInputForAnimation;
        }

        public void RefreshPile(PileRef pileRef, bool animate, int flipCardId = -1)
        {
            FixedCardPileState pile = _context.BoardState.GetPile(pileRef);
            SolitaireSlotAnchor slot = _context.ViewRegistry.GetSlot(pileRef);
            Vector3 basePosition = slot.transform.position;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;

            slot.SetSlotVisualVisible(SolitairePileLayoutLogic.Visibility.ShouldShowEmptySlotVisual(pile.Count));
            LockInputWhenNeeded(animate, pile.Count, flipCardId);

            Transform parent = slot.transform;
            for (int i = 0; i < pile.Count; i++)
                LayoutCardInPile(pileRef, pile, parent, basePosition, cardSize, i, animate, flipCardId);
        }

        public void PositionAllCardsAtStock()
        {
            SolitaireSlotAnchor stockSlot = _context.ViewRegistry.Stock;
            Vector3 stockBase = stockSlot.transform.position;
            FixedCardPileState stockPile = _context.BoardState.Stock;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;

            for (int cardId = 0; cardId < SolitaireCardUtility.CardCount; cardId++)
                PositionCardAtStock(stockSlot, stockBase, stockPile, cardSize, cardId);
        }

        private void LockInputWhenNeeded(bool animate, int pileCount, int flipCardId)
        {
            if (!SolitairePileLayoutLogic.Animation.ShouldLockInputForAnimation(animate, pileCount))
                return;

            float duration = SolitairePileLayoutLogic.Animation.GetPileAnimationDuration(_config, animate, flipCardId);
            _lockInputForAnimation?.Invoke(duration);
        }

        private void LayoutCardInPile(
            PileRef pileRef,
            FixedCardPileState pile,
            Transform parent,
            Vector3 basePosition,
            Vector2 cardSize,
            int pileIndex,
            bool animate,
            int flipCardId)
        {
            int cardId = pile[pileIndex];
            CardState state = _context.BoardState.GetCard(cardId);
            CardView card = _context.ViewRegistry.GetCard(cardId);
            Vector3 target = CalculatePosition(pileRef, basePosition, pileIndex, state);

            BindCardToPile(card, parent, cardSize, pileRef.Type, pile.Count, pileIndex);
            ApplyCardRefresh(card, state, target, animate, cardId, flipCardId);
        }

        private void BindCardToPile(
            CardView card,
            Transform parent,
            Vector2 cardSize,
            SolitairePileType pileType,
            int pileCount,
            int pileIndex)
        {
            card.CachedTransform.SetParent(parent, true);
            card.ApplyLayoutSize(cardSize);
            card.SetSortingOrder(SolitairePileLayoutLogic.Sorting.GetCardSortingOrder(_config.BaseSortingOrder, pileIndex));
            card.SetCardRendererVisible(SolitairePileLayoutLogic.Visibility.ShouldRenderCardInPile(pileType, pileCount, pileIndex));
        }

        private void ApplyCardRefresh(
            CardView card,
            CardState state,
            Vector3 target,
            bool animate,
            int cardId,
            int flipCardId)
        {
            SolitairePileLayoutLogic.PileCardRefreshMode mode =
                SolitairePileLayoutLogic.Placement.ResolveCardRefreshMode(animate, cardId, flipCardId, state.IsFaceUp);

            switch (mode)
            {
                case SolitairePileLayoutLogic.PileCardRefreshMode.AnimatedFlip:
                    PlayCardFlipPresentation(card, state, target);
                    return;
                case SolitairePileLayoutLogic.PileCardRefreshMode.AnimatedMove:
                    RefreshAndAnimateMove(card, state, target);
                    return;
                default:
                    RefreshInstant(card, state, target);
                    return;
            }
        }

        private void RefreshInstant(CardView card, CardState state, Vector3 target)
        {
            card.Refresh(state, _config);
            card.CachedTransform.position = target;
        }

        private void RefreshAndAnimateMove(CardView card, CardState state, Vector3 target)
        {
            card.Refresh(state, _config);
            card.MoveTo(target, _config.MoveAnimationDuration);
        }

        private void PlayCardFlipPresentation(CardView card, CardState state, Vector3 target)
        {
            card.ApplyBackFace(state, _config);
            DispatchFlipPresentation(card, state, target);
        }

        private void DispatchFlipPresentation(CardView card, CardState state, Vector3 target)
        {
            SolitairePileLayoutLogic.Animation.FlipPresentationKind kind =
                SolitairePileLayoutLogic.Animation.ResolveFlipPresentationKind(
                    card.CachedTransform.position,
                    target);

            switch (kind)
            {
                case SolitairePileLayoutLogic.Animation.FlipPresentationKind.MoveThenFlip:
                    PlayMoveThenFlip(card, state, target);
                    return;
                default:
                    PlayFlipReveal(card, state);
                    return;
            }
        }

        private void PlayMoveThenFlip(CardView card, CardState state, Vector3 target)
        {
            card.PlayMoveThenFlip(
                target,
                state,
                _config,
                _config.MoveAnimationDuration,
                _config.FlipAnimationDuration,
                SolitairePileLayoutLogic.Animation.GetFlipArcHeight(_config));
        }

        private void PlayFlipReveal(CardView card, CardState state)
        {
            card.PlayFlipReveal(state, _config, _config.FlipAnimationDuration);
        }

        private void PositionCardAtStock(
            SolitaireSlotAnchor stockSlot,
            Vector3 stockBase,
            FixedCardPileState stockPile,
            Vector2 cardSize,
            int cardId)
        {
            CardState state = _context.BoardState.GetCard(cardId);
            CardView card = _context.ViewRegistry.GetCard(cardId);

            BindCardAtStock(card, state, stockSlot, cardSize, stockPile, cardId);
            card.CachedTransform.position = ResolveStockCardPosition(state, stockPile, stockBase, cardId);
        }

        private void BindCardAtStock(
            CardView card,
            CardState state,
            SolitaireSlotAnchor stockSlot,
            Vector2 cardSize,
            FixedCardPileState stockPile,
            int cardId)
        {
            card.CachedTransform.SetParent(stockSlot.transform, true);
            card.ApplyLayoutSize(cardSize);
            card.ApplyBackFace(state, _config);
            card.SetSortingOrder(SolitairePileLayoutLogic.Sorting.GetCardSortingOrder(_config.BaseSortingOrder, cardId));
            card.SetCardRendererVisible(
                SolitairePileLayoutLogic.Visibility.ShouldRenderStockTopCard(
                    state.CurrentPileType,
                    stockPile.IsTopCard(cardId)));
        }

        private Vector3 ResolveStockCardPosition(
            CardState state,
            FixedCardPileState stockPile,
            Vector3 stockBase,
            int cardId)
        {
            if (!SolitairePileLayoutLogic.Placement.IsCardInStock(state.CurrentPileType))
                return stockBase;

            int stockIndex = stockPile.IndexOf(cardId);
            return CalculatePosition(
                new PileRef(SolitairePileType.Stock, 0),
                stockBase,
                stockIndex,
                state);
        }

        private Vector3 CalculatePosition(PileRef pileRef, Vector3 basePosition, int index, CardState card)
        {
            return SolitairePileLayoutOffsets.Calculate(
                _context.BoardState,
                _context.LayoutMetrics,
                _config,
                pileRef,
                basePosition,
                index,
                card);
        }
    }
}
