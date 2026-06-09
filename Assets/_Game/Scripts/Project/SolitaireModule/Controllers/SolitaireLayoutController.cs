using System;
using System.Collections;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Presentation;
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
        private SolitaireLayoutAnimationLock _animationLock;
        private SolitaireResponsiveBoardLayout _responsiveLayout;
        private SolitairePileLayoutPresenter _pileLayoutPresenter;
        private SolitaireInitialDealPresenter _initialDealPresenter;
        private SolitaireWinCelebrationPresenter _winCelebrationPresenter;
        private Coroutine _dealRoutine;
        private Coroutine _winCelebrationRoutine;
        private Coroutine _viewportRefreshRoutine;
        private Action _dealCompletedCallback;

        public void Initialize(SolitaireDeckConfigSO config, SolitaireRuntimeContext context)
        {
            _config = config;
            _context = context;
            _context.LayoutMetrics.ResetToConfig(_config);

            _animationLock = new SolitaireLayoutAnimationLock(this, _context);
            _responsiveLayout = new SolitaireResponsiveBoardLayout();
            _responsiveLayout.Initialize(_config, _context);
            _pileLayoutPresenter = new SolitairePileLayoutPresenter();
            _pileLayoutPresenter.Initialize(_config, _context, LockInputForAnimation);
            _initialDealPresenter = new SolitaireInitialDealPresenter();
            _initialDealPresenter.Initialize(_config, _context, _responsiveLayout, _pileLayoutPresenter, ResolveLayoutCamera, LockInputForAnimation);
            _winCelebrationPresenter = new SolitaireWinCelebrationPresenter();
            _winCelebrationPresenter.Initialize(_config, _context, LockInputForAnimation);
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

            _animationLock?.Cancel();
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
            _pileLayoutPresenter.RefreshPile(pileRef, animate, flipCardId);
        }

        private IEnumerator PlayInitialDealRoutine()
        {
            yield return _initialDealPresenter.PlayRoutine();

            _dealRoutine = null;
            Action callback = _dealCompletedCallback;
            _dealCompletedCallback = null;
            callback?.Invoke();
        }

        private IEnumerator PlayWinCelebrationRoutine(Action onCompleted)
        {
            yield return _winCelebrationPresenter.PlayRoutine(onCompleted);
            _winCelebrationRoutine = null;
        }

        private void ApplyResponsiveLayout()
        {
            _responsiveLayout.Apply(ResolveLayoutCamera());
        }

        private Camera ResolveLayoutCamera()
        {
            return _boardCameraController != null ? _boardCameraController.Camera : null;
        }

        private void LockInputForAnimation(float duration)
        {
            _animationLock.LockFor(duration);
        }

        private void EnsureInitialized()
        {
            if (_config == null || _context == null || _pileLayoutPresenter == null)
                throw new InvalidOperationException($"{nameof(SolitaireLayoutController)} is not initialized.");
        }
    }
}
