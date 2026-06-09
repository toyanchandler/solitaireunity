using System.Collections;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class SolitaireSlotAnchor : MonoBehaviour
    {
        [SerializeField] private SolitairePileType pileType;
        [SerializeField] private int pileIndex;
        [SerializeField] private CardSuit foundationSuit;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private SpriteRenderer highlightRenderer;

        private Coroutine _pulseRoutine;
        private Vector3 _homeScale = Vector3.one;
        private bool _isSlotVisualVisible = true;

        public SolitairePileType PileType => pileType;
        public int PileIndex => pileIndex;
        public CardSuit FoundationSuit => foundationSuit;
        public BoxCollider2D BoxCollider => boxCollider;
        public PileRef PileRef => new PileRef(pileType, pileIndex);

        public void Configure(SolitairePileType newPileType, int newPileIndex, CardSuit newFoundationSuit)
        {
            pileType = newPileType;
            pileIndex = newPileIndex;
            foundationSuit = newFoundationSuit;
        }

        private void Awake()
        {
            _homeScale = transform.localScale;
        }

        private void OnEnable()
        {
            SolitaireFeatureRegistration.RegisterSlot(this);
        }

        private void OnDestroy()
        {
            SolitaireFeatureRegistration.UnregisterSlot(this);
        }

        public void SetHighlight(bool isHighlighted, Color color)
        {
            if (!SolitaireSlotAnchorLogic.Highlight.CanApply(highlightRenderer))
                return;

            highlightRenderer.enabled = SolitaireSlotAnchorLogic.Highlight.ShouldEnableRenderer(
                isHighlighted,
                _isSlotVisualVisible);
            highlightRenderer.color = SolitaireSlotAnchorLogic.Colors.WithOpaqueAlpha(
                SolitaireSlotAnchorLogic.Colors.ResolveHighlightColor(isHighlighted, color));
        }

        public void SetSlotVisualVisible(bool isVisible)
        {
            _isSlotVisualVisible = isVisible;

            if (SolitaireSlotAnchorLogic.Highlight.CanApply(highlightRenderer))
                highlightRenderer.enabled = isVisible;
        }

        public void PlayFoundationPulse(SolitaireDeckConfigSO config)
        {
            if (!SolitaireSlotAnchorLogic.Pulse.Guards.CanPlayFoundation(config, pileType))
                return;

            RestartPulseRoutine(config);
        }

        public void PlayWinPulse(SolitaireDeckConfigSO config)
        {
            if (!SolitaireSlotAnchorLogic.Pulse.Guards.CanPlayWin(config))
                return;

            RestartPulseRoutine(config);
        }

        public void ApplyLayoutSize(Vector2 worldSize)
        {
            ApplyColliderSize(worldSize);
            ApplyHighlightLayout(worldSize);
        }

        public void ApplyTableauColumnDropArea(Vector2 cardSize, float columnBottomY)
        {
            ApplyTableauCollider(cardSize, columnBottomY);
            ApplyLayoutSize(cardSize);
        }

        public bool Validate(out string error)
        {
            (bool isValid, string validationError) = SolitaireSlotAnchorLogic.Validation.Validate(name, boxCollider);
            error = validationError;
            return isValid;
        }

        private void RestartPulseRoutine(SolitaireDeckConfigSO config)
        {
            StopActivePulseRoutine();
            _pulseRoutine = StartCoroutine(FoundationPulseRoutine(config));
        }

        private void StopActivePulseRoutine()
        {
            if (_pulseRoutine == null)
                return;

            StopCoroutine(_pulseRoutine);
        }

        private void ApplyColliderSize(Vector2 worldSize)
        {
            if (!SolitaireSlotAnchorLogic.Layout.CanApplyCollider(boxCollider))
                return;

            boxCollider.size = worldSize;
        }

        private void ApplyHighlightLayout(Vector2 worldSize)
        {
            if (!SolitaireSlotAnchorLogic.Layout.CanApplyHighlightLayout(highlightRenderer))
                return;

            transform.localScale = Vector3.one;
            highlightRenderer.drawMode = SpriteDrawMode.Sliced;
            highlightRenderer.size = worldSize;
        }

        private void ApplyTableauCollider(Vector2 cardSize, float columnBottomY)
        {
            if (!SolitaireSlotAnchorLogic.Layout.CanApplyCollider(boxCollider))
                return;

            (Vector2 size, Vector2 offset) = SolitaireSlotAnchorLogic.Layout.TableauDropArea.Compute(
                transform.position,
                cardSize,
                columnBottomY);
            boxCollider.size = size;
            boxCollider.offset = offset;
        }

        private IEnumerator FoundationPulseRoutine(SolitaireDeckConfigSO config)
        {
            if (!SolitaireSlotAnchorLogic.Pulse.Guards.CanRunRoutine(highlightRenderer))
            {
                _pulseRoutine = null;
                yield break;
            }

            float duration = config.FoundationPulseDuration;
            float targetScale = config.FoundationPulseScale;
            Vector2 baseSize = SolitaireSlotAnchorLogic.Pulse.ScaleOps.ResolveBaseSize(
                highlightRenderer.size,
                config.CardSize);
            SolitaireSlotAnchorLogic.Pulse.SpriteState spriteState =
                SolitaireSlotAnchorLogic.Pulse.SpriteState.Capture(highlightRenderer);
            Color startColor = SolitaireSlotAnchorLogic.Colors.CreatePulseStartColor();
            float elapsed = 0f;

            spriteState.ApplyTo(highlightRenderer);
            highlightRenderer.enabled = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SolitaireSlotAnchorLogic.Pulse.FrameState.ApplyVisual(
                    highlightRenderer,
                    SolitaireSlotAnchorLogic.Pulse.FrameState.Compute(
                        elapsed,
                        duration,
                        targetScale,
                        startColor,
                        spriteState.UseSpriteScale),
                    baseSize);
                yield return null;
            }

            spriteState.RestoreTo(
                highlightRenderer,
                baseSize,
                SolitaireSlotAnchorLogic.Constants.DefaultSlot,
                _isSlotVisualVisible);
            _pulseRoutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            highlightRenderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
