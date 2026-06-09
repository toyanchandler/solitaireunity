using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Screens
{
    public sealed class SolitaireHudLayoutAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform movesCounter;
        [SerializeField] private RectTransform scoreCounter;
        [SerializeField] private RectTransform undoButton;
        [SerializeField] private RectTransform hintButton;
        [SerializeField] private RectTransform autoCompleteButton;
        [SerializeField] private Button undoButtonComponent;
        [SerializeField] private Button hintButtonComponent;
        [SerializeField] private Button autoCompleteButtonComponent;

        [Header("Portrait")]
        [SerializeField] private Vector2 portraitCounterSize = new Vector2(146f, 46f);
        [SerializeField] private Vector2 portraitButtonSize = new Vector2(106f, 46f);
        [SerializeField] private float portraitCounterY = 0.925f;
        [SerializeField] private float portraitButtonY = 0.065f;

        [Header("Landscape")]
        [SerializeField] private Vector2 landscapeCounterSize = new Vector2(132f, 42f);
        [SerializeField] private Vector2 landscapeButtonSize = new Vector2(106f, 42f);
        [SerializeField] private float landscapeY = 0.92f;

        [Header("Animation")]
        [SerializeField] private float layoutTweenDuration = 0.18f;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.12f;

        private RectTransform _root;
        private bool _hasAppliedLayout;
        private bool _isLandscape;
        private Sequence _scorePulse;
        private Sequence _undoPulse;
        private Sequence _hintPulse;
        private Sequence _autoCompletePulse;

        private void Awake()
        {
            _root = (RectTransform)transform;
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.ScoreActionPerformed += HandleScoreAction;

            if (undoButtonComponent != null)
                undoButtonComponent.onClick.AddListener(HandleUndoClicked);

            if (hintButtonComponent != null)
                hintButtonComponent.onClick.AddListener(HandleHintClicked);

            if (autoCompleteButtonComponent != null)
                autoCompleteButtonComponent.onClick.AddListener(HandleAutoCompleteClicked);

            EventManager.SolitaireEvents.HintShown += HandleHintShown;
            EventManager.SolitaireEvents.AutoCompleteCompleted += HandleAutoCompleteCompleted;
            ApplyLayout(false);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.ScoreActionPerformed -= HandleScoreAction;

            if (undoButtonComponent != null)
                undoButtonComponent.onClick.RemoveListener(HandleUndoClicked);

            if (hintButtonComponent != null)
                hintButtonComponent.onClick.RemoveListener(HandleHintClicked);

            if (autoCompleteButtonComponent != null)
                autoCompleteButtonComponent.onClick.RemoveListener(HandleAutoCompleteClicked);

            EventManager.SolitaireEvents.HintShown -= HandleHintShown;
            EventManager.SolitaireEvents.AutoCompleteCompleted -= HandleAutoCompleteCompleted;
            _scorePulse?.Kill();
            _undoPulse?.Kill();
            _hintPulse?.Kill();
            _autoCompletePulse?.Kill();
            KillLayoutTweens();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;

            ApplyLayout(true);
        }

        private void ApplyLayout(bool animate)
        {
            if (_root == null)
                _root = (RectTransform)transform;

            Rect rect = _root.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            bool nextLandscape = rect.width >= rect.height;
            bool orientationChanged = _hasAppliedLayout && nextLandscape != _isLandscape;
            _isLandscape = nextLandscape;

            if (_isLandscape)
            {
                ApplyItemLayout(movesCounter, new Vector2(0.13f, landscapeY), landscapeCounterSize, animate && orientationChanged);
                ApplyItemLayout(scoreCounter, new Vector2(0.32f, landscapeY), landscapeCounterSize, animate && orientationChanged);
                ApplyItemLayout(undoButton, new Vector2(0.52f, landscapeY), landscapeButtonSize, animate && orientationChanged);
                ApplyItemLayout(hintButton, new Vector2(0.69f, landscapeY), landscapeButtonSize, animate && orientationChanged);
                ApplyItemLayout(autoCompleteButton, new Vector2(0.86f, landscapeY), landscapeButtonSize, animate && orientationChanged);
            }
            else
            {
                ApplyItemLayout(movesCounter, new Vector2(0.28f, portraitCounterY), portraitCounterSize, animate && orientationChanged);
                ApplyItemLayout(scoreCounter, new Vector2(0.72f, portraitCounterY), portraitCounterSize, animate && orientationChanged);
                ApplyItemLayout(undoButton, new Vector2(0.2f, portraitButtonY), portraitButtonSize, animate && orientationChanged);
                ApplyItemLayout(hintButton, new Vector2(0.5f, portraitButtonY), portraitButtonSize, animate && orientationChanged);
                ApplyItemLayout(autoCompleteButton, new Vector2(0.8f, portraitButtonY), portraitButtonSize, animate && orientationChanged);
            }

            _hasAppliedLayout = true;
        }

        private void ApplyItemLayout(RectTransform item, Vector2 anchor, Vector2 size, bool animate)
        {
            if (item == null)
                return;

            item.anchorMin = anchor;
            item.anchorMax = anchor;
            item.pivot = new Vector2(0.5f, 0.5f);

            if (animate)
            {
                item.DOAnchorPos(Vector2.zero, layoutTweenDuration).SetEase(Ease.OutCubic);
                item.DOSizeDelta(size, layoutTweenDuration).SetEase(Ease.OutCubic);
                return;
            }

            item.anchoredPosition = Vector2.zero;
            item.sizeDelta = size;
        }

        private void HandleScoreAction(SolitaireScoreAction _)
        {
            Pulse(scoreCounter, ref _scorePulse);
        }

        private void HandleUndoClicked()
        {
            Pulse(undoButton, ref _undoPulse);
        }

        private void HandleHintClicked()
        {
            Pulse(hintButton, ref _hintPulse);
        }

        private void HandleAutoCompleteClicked()
        {
            Pulse(autoCompleteButton, ref _autoCompletePulse);
        }

        private void HandleHintShown(SolitaireHint hint)
        {
            if (hint.IsValid)
                Pulse(hintButton, ref _hintPulse);
        }

        private void HandleAutoCompleteCompleted(int completedMoveCount)
        {
            if (completedMoveCount > 0)
                Pulse(autoCompleteButton, ref _autoCompletePulse);
        }

        private void Pulse(RectTransform target, ref Sequence sequence)
        {
            if (target == null)
                return;

            sequence?.Kill();
            target.localScale = Vector3.one;
            sequence = DOTween.Sequence()
                .Append(target.DOScale(pulseScale, pulseDuration).SetEase(Ease.OutBack))
                .Append(target.DOScale(1f, pulseDuration).SetEase(Ease.OutCubic));
        }

        private void KillLayoutTweens()
        {
            movesCounter?.DOKill();
            scoreCounter?.DOKill();
            undoButton?.DOKill();
            hintButton?.DOKill();
            autoCompleteButton?.DOKill();
        }
    }
}
