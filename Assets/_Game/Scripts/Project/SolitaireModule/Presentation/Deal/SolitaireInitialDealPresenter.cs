using System;
using System.Collections;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitaireInitialDealPresenter
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private SolitaireResponsiveBoardLayout _responsiveLayout;
        private SolitairePileLayoutPresenter _pileLayoutPresenter;
        private Func<Camera> _resolveCamera;
        private Action<float> _lockInputForAnimation;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            SolitaireResponsiveBoardLayout responsiveLayout,
            SolitairePileLayoutPresenter pileLayoutPresenter,
            Func<Camera> resolveCamera,
            Action<float> lockInputForAnimation)
        {
            _config = config;
            _context = context;
            _responsiveLayout = responsiveLayout;
            _pileLayoutPresenter = pileLayoutPresenter;
            _resolveCamera = resolveCamera;
            _lockInputForAnimation = lockInputForAnimation;
        }

        public IEnumerator PlayRoutine()
        {
            PrepareBoardBeforeDeal();
            LockInputForEntireDeal();

            yield return DealAllTableauColumns();
            yield return WaitForFinalDealAnimations();

            RefreshAllPilesAfterDeal();
        }

        private void PrepareBoardBeforeDeal()
        {
            _responsiveLayout.Apply(_resolveCamera?.Invoke());
            _pileLayoutPresenter.PositionAllCardsAtStock();
        }

        private void LockInputForEntireDeal()
        {
            float lockDuration = SolitaireInitialDealLogic.Timing.CalculateInputLockDuration(
                SolitaireInitialDealLogic.DealSteps.CountForDefaultTableau(),
                _config.DealStaggerDelay,
                _config.DealAnimationDuration,
                _config.FlipAnimationDuration);

            _lockInputForAnimation?.Invoke(lockDuration);
        }

        private IEnumerator DealAllTableauColumns()
        {
            for (int column = 0; column < SolitaireCardUtility.TableauCount; column++)
                yield return DealTableauColumn(column);
        }

        private IEnumerator DealTableauColumn(int column)
        {
            PileRef pileRef = SolitaireInitialDealLogic.TableauColumn.CreatePileRef(column);
            FixedCardPileState pile = _context.BoardState.GetPile(pileRef);
            SolitaireSlotAnchor slot = _context.ViewRegistry.GetSlot(pileRef);

            slot.SetSlotVisualVisible(
                SolitaireInitialDealLogic.TableauColumn.ShouldShowSlotVisual(pile.Count));

            Vector3 basePosition = slot.transform.position;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;

            for (int row = 0; row <= column; row++)
            {
                PlayTableauCardDeal(column, row, pile, slot, basePosition, cardSize);
                yield return new WaitForSeconds(_config.DealStaggerDelay);
            }
        }

        private void PlayTableauCardDeal(
            int column,
            int row,
            FixedCardPileState pile,
            SolitaireSlotAnchor slot,
            Vector3 basePosition,
            Vector2 cardSize)
        {
            int cardId = pile[row];
            CardState state = _context.BoardState.GetCard(cardId);
            CardView card = _context.ViewRegistry.GetCard(cardId);
            PileRef pileRef = SolitaireInitialDealLogic.TableauColumn.CreatePileRef(column);

            Vector3 target = SolitairePileLayoutOffsets.Calculate(
                _context.BoardState,
                _context.LayoutMetrics,
                _config,
                pileRef,
                basePosition,
                row,
                state);

            card.CachedTransform.SetParent(slot.transform, true);
            card.ApplyLayoutSize(cardSize);
            card.SetSortingOrder(
                SolitaireInitialDealLogic.TableauColumn.CalculateSortingOrder(
                    _config.BaseSortingOrder,
                    row,
                    column));
            card.SetCardRendererVisible(true);
            card.PlayDealMove(
                target,
                state,
                _config,
                _config.DealAnimationDuration,
                _config.FlipAnimationDuration,
                _config.DealArcHeight,
                SolitaireInitialDealLogic.TableauColumn.ShouldFlipOnLand(row, column));
        }

        private IEnumerator WaitForFinalDealAnimations()
        {
            float waitDuration = SolitaireInitialDealLogic.Timing.CalculatePostDealWaitDuration(
                _config.DealAnimationDuration,
                _config.FlipAnimationDuration);

            yield return new WaitForSeconds(waitDuration);
        }

        private void RefreshAllPilesAfterDeal()
        {
            _pileLayoutPresenter.RefreshPile(SolitaireInitialDealLogic.PostDealRefresh.Stock, false);
            _pileLayoutPresenter.RefreshPile(SolitaireInitialDealLogic.PostDealRefresh.Waste, false);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                _pileLayoutPresenter.RefreshPile(
                    SolitaireInitialDealLogic.PostDealRefresh.CreateFoundationPileRef(i),
                    false);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
                _pileLayoutPresenter.RefreshPile(
                    SolitaireInitialDealLogic.PostDealRefresh.CreateTableauPileRef(i),
                    false);
        }
    }
}
