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
            if (highlightRenderer == null)
                return;

            ApplyHighlightColor(isHighlighted ? color : GetDefaultSlotColor());
        }

        public void PlayFoundationPulse(SolitaireDeckConfigSO config)
        {
            if (config == null || pileType != SolitairePileType.Foundation)
                return;

            if (_pulseRoutine != null)
                StopCoroutine(_pulseRoutine);

            _pulseRoutine = StartCoroutine(FoundationPulseRoutine(config));
        }

        public void PlayWinPulse(SolitaireDeckConfigSO config)
        {
            if (config == null)
                return;

            if (_pulseRoutine != null)
                StopCoroutine(_pulseRoutine);

            _pulseRoutine = StartCoroutine(FoundationPulseRoutine(config));
        }

        public void ApplyLayoutSize(Vector2 worldSize)
        {
            if (boxCollider != null)
                boxCollider.size = worldSize;

            if (highlightRenderer == null)
                return;

            transform.localScale = Vector3.one;
            highlightRenderer.drawMode = SpriteDrawMode.Sliced;
            highlightRenderer.size = worldSize;
        }

        public void ApplyTableauColumnDropArea(Vector2 cardSize, float columnBottomY)
        {
            if (boxCollider != null)
            {
                float columnTopY = transform.position.y + (cardSize.y * 0.5f);
                float columnHeight = Mathf.Max(cardSize.y, columnTopY - columnBottomY);
                boxCollider.size = new Vector2(cardSize.x, columnHeight);
                boxCollider.offset = new Vector2(0f, -(columnHeight - cardSize.y) * 0.5f);
            }

            ApplyLayoutSize(cardSize);
        }

        private IEnumerator FoundationPulseRoutine(SolitaireDeckConfigSO config)
        {
            if (highlightRenderer == null)
            {
                _pulseRoutine = null;
                yield break;
            }

            float duration = config.FoundationPulseDuration;
            float targetScale = config.FoundationPulseScale;
            Vector2 baseSize = highlightRenderer.size;
            if (baseSize.x <= 0f || baseSize.y <= 0f)
                baseSize = new Vector2(0.52f, 0.74f);

            Sprite previousSprite = highlightRenderer.sprite;
            SpriteDrawMode previousDrawMode = highlightRenderer.drawMode;
            Sprite pulseSprite = Resources.Load<Sprite>("Solitaire/card_selection_highlight");

            if (pulseSprite != null)
            {
                highlightRenderer.sprite = pulseSprite;
                highlightRenderer.drawMode = SpriteDrawMode.Simple;
            }
            else
            {
                highlightRenderer.drawMode = SpriteDrawMode.Sliced;
            }

            Color startColor = FoundationPulseColor;
            startColor.a = 0.95f;
            float elapsed = 0f;
            highlightRenderer.enabled = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                float scale = Mathf.Lerp(0.82f, targetScale, eased);

                if (pulseSprite != null)
                    highlightRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                else
                    highlightRenderer.size = baseSize * scale;

                Color color = startColor;
                color.a = startColor.a * (1f - t);
                ApplyHighlightColor(color);

                yield return null;
            }

            highlightRenderer.transform.localScale = Vector3.one;
            highlightRenderer.size = baseSize;
            highlightRenderer.sprite = previousSprite;
            highlightRenderer.drawMode = previousDrawMode;
            ApplyHighlightColor(GetDefaultSlotColor());
            _pulseRoutine = null;
        }

        private static readonly Color DefaultSlotColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color FoundationPulseColor = new Color(0.35f, 0.72f, 1f, 0.9f);

        private Color GetDefaultSlotColor() => DefaultSlotColor;

        private void ApplyHighlightColor(Color color)
        {
            color.a = 1f;
            highlightRenderer.color = color;
        }

        public bool Validate(out string error)
        {
            if (boxCollider == null)
            {
                error = $"{name} is missing BoxCollider2D.";
                return false;
            }

            error = string.Empty;
            return true;
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
