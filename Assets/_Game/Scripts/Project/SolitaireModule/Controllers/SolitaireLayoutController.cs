using System;
using System.Collections;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireLayoutController : MonoBehaviour
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private SolitaireBoardCameraController _boardCameraController;
        private Coroutine _animationLockRoutine;
        private Coroutine _dealRoutine;
        private Coroutine _winCelebrationRoutine;
        private Coroutine _viewportRefreshRoutine;
        private Action _dealCompletedCallback;

        public void Initialize(SolitaireDeckConfigSO config, SolitaireRuntimeContext context)
        {
            _config = config;
            _context = context;
            _context.LayoutMetrics.ResetToConfig(_config);
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.BoardCameraReady += HandleBoardCameraReady;
            EventManager.SolitaireEvents.BoardViewportSizeChanged += OnGameViewportSizeChanged;

            if (SolitaireFeatureRegistration.BoardCamera != null)
                HandleBoardCameraReady(SolitaireFeatureRegistration.BoardCamera);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.BoardCameraReady -= HandleBoardCameraReady;
            EventManager.SolitaireEvents.BoardViewportSizeChanged -= OnGameViewportSizeChanged;
            _boardCameraController = null;
        }

        private void HandleBoardCameraReady(SolitaireBoardCameraController boardCameraController)
        {
            _boardCameraController = boardCameraController;
        }

        private void OnDestroy()
        {
            if (_viewportRefreshRoutine != null)
                StopCoroutine(_viewportRefreshRoutine);
        }

        private void OnGameViewportSizeChanged()
        {
            if (_config == null || _context == null)
                return;

            if (_viewportRefreshRoutine != null)
                StopCoroutine(_viewportRefreshRoutine);

            _viewportRefreshRoutine = StartCoroutine(RefreshAfterViewportSettled());
        }

        private IEnumerator RefreshAfterViewportSettled()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            if (_config == null || _context == null)
            {
                _viewportRefreshRoutine = null;
                yield break;
            }

            RefreshAll(false);
            _viewportRefreshRoutine = null;
        }

        public void RefreshAll(bool animate, int flipCardId = -1)
        {
            EnsureInitialized();
            ApplyResponsiveLayout();
            RefreshPile(new PileRef(SolitairePileType.Stock, 0), animate, flipCardId);
            RefreshPile(new PileRef(SolitairePileType.Waste, 0), animate, flipCardId);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                RefreshPile(new PileRef(SolitairePileType.Foundation, i), animate, flipCardId);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
                RefreshPile(new PileRef(SolitairePileType.Tableau, i), animate, flipCardId);
        }

        public void CancelActivePresentations()
        {
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
                _dealRoutine = null;
                _dealCompletedCallback = null;
            }

            if (_winCelebrationRoutine != null)
            {
                StopCoroutine(_winCelebrationRoutine);
                _winCelebrationRoutine = null;
            }

            if (_animationLockRoutine != null)
            {
                StopCoroutine(_animationLockRoutine);
                _animationLockRoutine = null;
            }

            if (_context != null)
                _context.EndAnimationLock();
        }

        public void PlayInitialDeal(Action onCompleted = null)
        {
            EnsureInitialized();
            _dealCompletedCallback = onCompleted;

            if (_dealRoutine != null)
                StopCoroutine(_dealRoutine);

            _dealRoutine = StartCoroutine(PlayInitialDealRoutine());
        }

        public void PlayFoundationPulse(PileRef foundationRef)
        {
            EnsureInitialized();

            if (foundationRef.Type != SolitairePileType.Foundation)
                return;

            SolitaireSlotAnchor slot = _context.ViewRegistry.GetSlot(foundationRef);
            slot?.PlayFoundationPulse(_config);
        }

        public void PlayWinCelebration(Action onCompleted = null)
        {
            EnsureInitialized();

            if (_winCelebrationRoutine != null)
                StopCoroutine(_winCelebrationRoutine);

            _winCelebrationRoutine = StartCoroutine(PlayWinCelebrationRoutine(onCompleted));
        }

        public void RefreshPile(PileRef pileRef, bool animate, int flipCardId = -1)
        {
            EnsureInitialized();

            FixedCardPileState pile = _context.BoardState.GetPile(pileRef);
            SolitaireSlotAnchor slot = _context.ViewRegistry.GetSlot(pileRef);
            Vector3 basePosition = slot.transform.position;
            float animationDuration = GetPileAnimationDuration(animate, flipCardId);
            Vector2 cardSize = _context.LayoutMetrics.CardSize;
            slot.SetSlotVisualVisible(pile.Count == 0);

            if (animate && pile.Count > 0)
                LockInputForAnimation(animationDuration);

            for (int i = 0; i < pile.Count; i++)
            {
                int cardId = pile[i];
                CardState state = _context.BoardState.GetCard(cardId);
                CardView card = _context.ViewRegistry.GetCard(cardId);
                Vector3 target = CalculatePosition(pileRef, basePosition, i, state);
                card.CachedTransform.SetParent(GetParentForPile(pileRef), true);
                card.ApplyLayoutSize(cardSize);
                card.SetSortingOrder(_config.BaseSortingOrder + i);
                card.SetCardRendererVisible(ShouldRenderCardInPile(pileRef, pile, i));

                if (animate && cardId == flipCardId && state.IsFaceUp)
                    PlayCardFlipPresentation(card, state, target);
                else if (animate)
                {
                    card.Refresh(state, _config);
                    card.MoveTo(target, _config.MoveAnimationDuration);
                }
                else
                {
                    card.Refresh(state, _config);
                    card.CachedTransform.position = target;
                }
            }
        }

        private IEnumerator PlayInitialDealRoutine()
        {
            ApplyResponsiveLayout();
            PositionAllCardsAtStock();

            int dealSteps = 0;

            for (int column = 0; column < SolitaireCardUtility.TableauCount; column++)
            {
                for (int row = 0; row <= column; row++)
                    dealSteps++;
            }

            float totalDuration = (dealSteps * _config.DealStaggerDelay) +
                                  (_config.DealAnimationDuration + _config.FlipAnimationDuration) +
                                  0.1f;
            LockInputForAnimation(totalDuration);

            Vector2 cardSize = _context.LayoutMetrics.CardSize;

            for (int column = 0; column < SolitaireCardUtility.TableauCount; column++)
            {
                FixedCardPileState pile = _context.BoardState.GetPile(new PileRef(SolitairePileType.Tableau, column));
                SolitaireSlotAnchor slot = _context.ViewRegistry.GetSlot(new PileRef(SolitairePileType.Tableau, column));
                slot.SetSlotVisualVisible(pile.Count == 0);
                Vector3 basePosition = slot.transform.position;

                for (int row = 0; row <= column; row++)
                {
                    int cardId = pile[row];
                    CardState state = _context.BoardState.GetCard(cardId);
                    CardView card = _context.ViewRegistry.GetCard(cardId);
                    Vector3 target = CalculatePosition(new PileRef(SolitairePileType.Tableau, column), basePosition, row, state);
                    bool flipOnLand = row == column;

                    card.CachedTransform.SetParent(slot.transform, true);
                    card.ApplyLayoutSize(cardSize);
                    card.SetSortingOrder(_config.BaseSortingOrder + row + (column * 4));
                    card.SetCardRendererVisible(true);
                    card.PlayDealMove(
                        target,
                        state,
                        _config,
                        _config.DealAnimationDuration,
                        _config.FlipAnimationDuration,
                        _config.DealArcHeight,
                        flipOnLand);

                    yield return new WaitForSeconds(_config.DealStaggerDelay);
                }
            }

            yield return new WaitForSeconds(_config.DealAnimationDuration + _config.FlipAnimationDuration);
            RefreshPile(new PileRef(SolitairePileType.Stock, 0), false);
            RefreshPile(new PileRef(SolitairePileType.Waste, 0), false);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                RefreshPile(new PileRef(SolitairePileType.Foundation, i), false);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
                RefreshPile(new PileRef(SolitairePileType.Tableau, i), false);

            _dealRoutine = null;
            Action callback = _dealCompletedCallback;
            _dealCompletedCallback = null;
            callback?.Invoke();
        }

        private IEnumerator PlayWinCelebrationRoutine(Action onCompleted)
        {
            LockInputForAnimation(_config.WinCelebrationDuration);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                _context.ViewRegistry.Foundations[i]?.PlayWinPulse(_config);

            float popDuration = Mathf.Max(0.12f, _config.WinCardStaggerDelay * 4f);
            int launchedCards = 0;

            for (int foundationIndex = 0; foundationIndex < SolitaireCardUtility.FoundationCount; foundationIndex++)
            {
                FixedCardPileState pile = _context.BoardState.GetPile(new PileRef(SolitairePileType.Foundation, foundationIndex));

                for (int cardIndex = 0; cardIndex < pile.Count; cardIndex++)
                {
                    CardView card = _context.ViewRegistry.GetCard(pile[cardIndex]);
                    card.PlayWinPop(_config.WinCardPopHeight, popDuration);
                    launchedCards++;
                    yield return new WaitForSeconds(_config.WinCardStaggerDelay);
                }
            }

            if (launchedCards == 0)
                yield return new WaitForSeconds(0.25f);
            else
                yield return new WaitForSeconds(popDuration);

            _winCelebrationRoutine = null;
            onCompleted?.Invoke();
        }

        private void PositionAllCardsAtStock()
        {
            SolitaireSlotAnchor stockSlot = _context.ViewRegistry.Stock;
            Vector3 stockBase = stockSlot.transform.position;
            FixedCardPileState stockPile = _context.BoardState.Stock;
            Vector2 cardSize = _context.LayoutMetrics.CardSize;

            for (int cardId = 0; cardId < SolitaireCardUtility.CardCount; cardId++)
            {
                CardState state = _context.BoardState.GetCard(cardId);
                CardView card = _context.ViewRegistry.GetCard(cardId);
                card.CachedTransform.SetParent(stockSlot.transform, true);
                card.ApplyLayoutSize(cardSize);
                card.ApplyBackFace(state, _config);
                card.SetSortingOrder(_config.BaseSortingOrder + cardId);
                card.SetCardRendererVisible(state.CurrentPileType == SolitairePileType.Stock && stockPile.IsTopCard(cardId));

                if (state.CurrentPileType == SolitairePileType.Stock)
                {
                    int stockIndex = stockPile.IndexOf(cardId);
                    card.CachedTransform.position = CalculatePosition(
                        new PileRef(SolitairePileType.Stock, 0),
                        stockBase,
                        stockIndex,
                        state);
                }
                else
                {
                    card.CachedTransform.position = stockBase;
                }
            }
        }

        private void PlayCardFlipPresentation(CardView card, CardState state, Vector3 target)
        {
            card.ApplyBackFace(state, _config);

            if (Vector3.Distance(card.CachedTransform.position, target) > 0.02f)
            {
                card.PlayMoveThenFlip(
                    target,
                    state,
                    _config,
                    _config.MoveAnimationDuration,
                    _config.FlipAnimationDuration,
                    _config.DealArcHeight * 0.35f);
                return;
            }

            card.PlayFlipReveal(state, _config, _config.FlipAnimationDuration);
        }

        private float GetPileAnimationDuration(bool animate, int flipCardId)
        {
            if (!animate)
                return 0f;

            return flipCardId >= 0
                ? _config.MoveAnimationDuration + _config.FlipAnimationDuration
                : _config.MoveAnimationDuration;
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

        private static bool ShouldRenderCardInPile(PileRef pileRef, FixedCardPileState pile, int index)
        {
            switch (pileRef.Type)
            {
                case SolitairePileType.Stock:
                case SolitairePileType.Foundation:
                    return index == pile.Count - 1;
                case SolitairePileType.Waste:
                    return index >= pile.Count - 3;
                default:
                    return true;
            }
        }

        private Transform GetParentForPile(PileRef pileRef)
        {
            return _context.ViewRegistry.GetSlot(pileRef).transform;
        }

        private void ApplyResponsiveLayout()
        {
            Camera camera = _boardCameraController != null ? _boardCameraController.Camera : null;

            if (camera == null || !camera.orthographic || !SolitaireBoardLayoutCalculator.TryCalculateResponsive(camera, _config, out SolitaireBoardLayoutResult layout))
            {
                _context.LayoutMetrics.ResetToConfig(_config);
                ApplySlotSizes(_context.LayoutMetrics.CardSize);
                return;
            }

            _context.LayoutMetrics.Apply(layout);
            PositionSlot(_context.ViewRegistry.Stock, layout.StockPosition);
            PositionSlot(_context.ViewRegistry.Waste, layout.WastePosition);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                PositionSlot(_context.ViewRegistry.Foundations[i], layout.FoundationPositions[i]);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
                PositionSlot(_context.ViewRegistry.Tableaus[i], layout.TableauPositions[i]);

            ApplySlotSizes(layout.CardSize);
        }

        private void PositionSlot(SolitaireSlotAnchor slot, Vector3 position)
        {
            if (slot != null)
                slot.transform.position = position;
        }

        private void ApplySlotSizes(Vector2 cardSize)
        {
            _context.ViewRegistry.Stock?.ApplyLayoutSize(cardSize);
            _context.ViewRegistry.Waste?.ApplyLayoutSize(cardSize);

            for (int i = 0; i < _context.ViewRegistry.Foundations.Length; i++)
                _context.ViewRegistry.Foundations[i]?.ApplyLayoutSize(cardSize);

            float columnBottomY = _context.LayoutMetrics.TableauBottomPlayableY;

            for (int i = 0; i < _context.ViewRegistry.Tableaus.Length; i++)
                _context.ViewRegistry.Tableaus[i]?.ApplyTableauColumnDropArea(cardSize, columnBottomY);
        }

        private void LockInputForAnimation(float duration)
        {
            if (_animationLockRoutine != null)
                StopCoroutine(_animationLockRoutine);

            _context.BeginAnimationLock();
            _animationLockRoutine = StartCoroutine(UnlockInputAfter(duration));
        }

        private IEnumerator UnlockInputAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            _context.EndAnimationLock();
            _animationLockRoutine = null;
        }

        private void EnsureInitialized()
        {
            if (_config == null || _context == null)
                throw new InvalidOperationException($"{nameof(SolitaireLayoutController)} is not initialized.");
        }
    }
}
