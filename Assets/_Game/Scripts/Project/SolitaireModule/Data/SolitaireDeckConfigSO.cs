using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Data
{
    [CreateAssetMenu(
        menuName = "GameModules/Solitaire/Deck Config",
        fileName = "SolitaireDeckConfig")]
    public sealed class SolitaireDeckConfigSO : ScriptableObject
    {
        [Header("Prefab References")]
        [SerializeField] private CardView cardPrefab;

        [Header("Card Sprites")]
        [SerializeField] private SolitaireCardVisualCatalogSO cardVisualCatalog;
        [SerializeField] private Sprite cardFrontSprite;
        [SerializeField] private Sprite cardBackSprite;

        [Header("Layout")]
        [SerializeField] private Vector2 cardSize = new Vector2(0.52f, 0.74f);
        [SerializeField] private float cardAspectRatio = 1.45f;
        [SerializeField] private float cardHorizontalSpacingRatio = 0.10f;
        [SerializeField] private float maxResponsiveCardWidth = 1.08f;
        [SerializeField] private float boardHorizontalPadding = 0.28f;
        [SerializeField] private float boardTopHudPadding = 1.20f;
        [SerializeField] private float boardBottomPadding = 0.45f;
        [SerializeField] private float rowVerticalGap = 0.34f;
        [SerializeField] private float faceUpTableauYOffset = 0.22f;
        [SerializeField] private float faceDownTableauYOffset = 0.10f;
        [SerializeField] private float minCompressedFaceUpYOffset = 0.12f;
        [SerializeField] private float tableauBottomPlayableY = -3.85f;
        [SerializeField] private float wasteStackXOffset = 0.09f;
        [SerializeField] private float stockZStep = -0.005f;
        [SerializeField] private float cardZStep = -0.01f;

        [Header("Input")]
        [SerializeField] private float doubleTapThreshold = 0.25f;
        [SerializeField] private float dragStartThresholdWorld = 0.08f;
        [SerializeField] private float dropSnapDistance = 0.30f;
        [SerializeField] private bool enableTapSelection;

        [Header("Drop Target")]
        [SerializeField] private float dragProbeTopOffsetRatio = 0.72f;
        [SerializeField] private float dragProbeHorizontalOffsetRatio = 0.35f;
        [SerializeField] private float dragProbeCornerYOffsetRatio = 0.58f;
        [SerializeField] private float foundationDropHalfWidthRatio = 0.95f;
        [SerializeField] private float foundationDropHalfHeightRatio = 0.85f;
        [SerializeField] private float tableauColumnHalfWidthRatio = 0.62f;
        [SerializeField] private float tableauColumnTopPaddingRatio = 1.25f;
        [SerializeField] private float tableauColumnBottomPaddingRatio = 0.25f;

        [Header("Animation")]
        [SerializeField] private float moveAnimationDuration = 0.16f;
        [SerializeField] private float invalidMoveReturnDuration = 0.12f;
        [SerializeField] private float flipAnimationDuration = 0.15f;
        [SerializeField] private float dealAnimationDuration = 0.22f;
        [SerializeField] private float dealStaggerDelay = 0.045f;
        [SerializeField] private float dealArcHeight = 0.35f;
        [SerializeField] private float flipLiftHeight = 0.06f;
        [SerializeField] private float flipTiltDegrees = 9f;
        [SerializeField] private int baseSortingOrder = 100;
        [SerializeField] private int dragSortingOrder = 5000;
        [SerializeField] private Color validTargetHighlightColor = new Color(0.28f, 0.82f, 0.55f, 0.68f);
        [SerializeField] private float dragLiftScale = 1.06f;
        [SerializeField] private float dragAlpha = 0.94f;
        [SerializeField] private Vector2 dragShadowOffset = new Vector2(0.04f, -0.05f);
        [SerializeField] private float dragShadowAlpha = 0.28f;
        [SerializeField] private float foundationPulseDuration = 0.18f;
        [SerializeField] private float foundationPulseScale = 1.12f;
        [SerializeField] private float winCelebrationDuration = 2.4f;
        [SerializeField] private float winCardStaggerDelay = 0.03f;
        [SerializeField] private float winCardPopHeight = 0.42f;
        [SerializeField] private bool hapticsEnabled;

        [Header("Rules")]
        [SerializeField] private SolitaireDrawMode drawMode = SolitaireDrawMode.DrawOne;
        [SerializeField] private bool allowFoundationToTableau;
        [SerializeField] private bool autoFlipTableauTopCard = true;
        [SerializeField] private bool doubleTapMovesToFoundationOnly = true;
        [SerializeField] private bool allowWasteRecycle = true;
        [SerializeField] private bool useFixedDealSeed;
        [SerializeField] private int dealSeed = 104729;

        public CardView CardPrefab => cardPrefab;
        public SolitaireCardVisualCatalogSO CardVisualCatalog => cardVisualCatalog;
        public Sprite CardFrontSprite => cardFrontSprite;
        public Sprite CardBackSprite => cardBackSprite;
        public Vector2 CardSize => cardSize;
        public float CardAspectRatio => Mathf.Max(1f, cardAspectRatio);
        public float CardHorizontalSpacingRatio => Mathf.Clamp(cardHorizontalSpacingRatio, 0.02f, 0.35f);
        public float MaxResponsiveCardWidth => Mathf.Max(0.1f, maxResponsiveCardWidth);
        public float BoardHorizontalPadding => Mathf.Max(0f, boardHorizontalPadding);
        public float BoardTopHudPadding => Mathf.Max(0f, boardTopHudPadding);
        public float BoardBottomPadding => Mathf.Max(0f, boardBottomPadding);
        public float RowVerticalGap => Mathf.Max(0f, rowVerticalGap);
        public float FaceUpTableauYOffset => Mathf.Max(0.01f, faceUpTableauYOffset);
        public float FaceDownTableauYOffset => Mathf.Max(0.01f, faceDownTableauYOffset);
        public float MinCompressedFaceUpYOffset => Mathf.Max(0.01f, minCompressedFaceUpYOffset);
        public float TableauBottomPlayableY => tableauBottomPlayableY;
        public float WasteStackXOffset => wasteStackXOffset;
        public float StockZStep => stockZStep;
        public float CardZStep => cardZStep;
        public float DoubleTapThreshold => Mathf.Max(0.05f, doubleTapThreshold);
        public float DragStartThresholdWorld => Mathf.Max(0.01f, dragStartThresholdWorld);
        public float DropSnapDistance => Mathf.Max(0.01f, dropSnapDistance);
        public bool EnableTapSelection => enableTapSelection;
        public float DragProbeTopOffsetRatio => Mathf.Clamp01(dragProbeTopOffsetRatio);
        public float DragProbeHorizontalOffsetRatio => Mathf.Clamp01(dragProbeHorizontalOffsetRatio);
        public float DragProbeCornerYOffsetRatio => Mathf.Clamp01(dragProbeCornerYOffsetRatio);
        public float FoundationDropHalfWidthRatio => Mathf.Clamp(foundationDropHalfWidthRatio, 0.1f, 2f);
        public float FoundationDropHalfHeightRatio => Mathf.Clamp(foundationDropHalfHeightRatio, 0.1f, 2f);
        public float TableauColumnHalfWidthRatio => Mathf.Clamp(tableauColumnHalfWidthRatio, 0.1f, 2f);
        public float TableauColumnTopPaddingRatio => Mathf.Clamp(tableauColumnTopPaddingRatio, 0.1f, 3f);
        public float TableauColumnBottomPaddingRatio => Mathf.Clamp(tableauColumnBottomPaddingRatio, 0f, 2f);
        public float MoveAnimationDuration => Mathf.Max(0.01f, moveAnimationDuration);
        public float InvalidMoveReturnDuration => Mathf.Max(0.01f, invalidMoveReturnDuration);
        public float FlipAnimationDuration => Mathf.Clamp(flipAnimationDuration, 0.08f, 0.35f);
        public float DealAnimationDuration => Mathf.Clamp(dealAnimationDuration, 0.1f, 0.5f);
        public float DealStaggerDelay => Mathf.Clamp(dealStaggerDelay, 0f, 0.2f);
        public float DealArcHeight => Mathf.Max(0f, dealArcHeight);
        public float FlipLiftHeight => Mathf.Max(0f, flipLiftHeight);
        public float FlipTiltDegrees => Mathf.Clamp(flipTiltDegrees, 0f, 20f);
        public int BaseSortingOrder => baseSortingOrder;
        public int DragSortingOrder => dragSortingOrder;
        public Color ValidTargetHighlightColor => validTargetHighlightColor;
        public float DragLiftScale => Mathf.Clamp(dragLiftScale, 1f, 1.2f);
        public float DragAlpha => Mathf.Clamp01(dragAlpha);
        public Vector2 DragShadowOffset => dragShadowOffset;
        public float DragShadowAlpha => Mathf.Clamp01(dragShadowAlpha);
        public float FoundationPulseDuration => Mathf.Clamp(foundationPulseDuration, 0.08f, 0.4f);
        public float FoundationPulseScale => Mathf.Clamp(foundationPulseScale, 1f, 1.35f);
        public float WinCelebrationDuration => Mathf.Clamp(winCelebrationDuration, 0.8f, 6f);
        public float WinCardStaggerDelay => Mathf.Clamp(winCardStaggerDelay, 0.01f, 0.12f);
        public float WinCardPopHeight => Mathf.Max(0.05f, winCardPopHeight);
        public bool HapticsEnabled => hapticsEnabled;
        public SolitaireDrawMode DrawMode => drawMode;
        public bool AllowFoundationToTableau => allowFoundationToTableau;
        public bool AutoFlipTableauTopCard => autoFlipTableauTopCard;
        public bool DoubleTapMovesToFoundationOnly => doubleTapMovesToFoundationOnly;
        public bool AllowWasteRecycle => allowWasteRecycle;
        public bool UseFixedDealSeed => useFixedDealSeed;
        public int DealSeed => dealSeed;

        public Sprite GetCardFrontSprite(CardState card)
        {
            if (cardVisualCatalog != null)
            {
                Sprite sprite = cardVisualCatalog.GetFrontSprite(card.Suit, card.Rank);
                if (sprite != null)
                    return sprite;
            }

            return cardFrontSprite;
        }

        public Sprite GetCardBackSprite(CardState card)
        {
            if (cardVisualCatalog != null)
            {
                Sprite sprite = cardVisualCatalog.GetBackSprite(card.Suit, card.Rank);
                if (sprite != null)
                    return sprite;
            }

            return cardBackSprite;
        }

        public bool Validate(out string error)
        {
            if (cardPrefab == null)
            {
                error = $"{name} is missing CardPrefab.";
                return false;
            }

            if (cardVisualCatalog != null && !cardVisualCatalog.ValidateComplete(out error))
                return false;

            if (cardVisualCatalog == null && cardFrontSprite == null)
            {
                error = $"{name} is missing CardVisualCatalog or CardFrontSprite.";
                return false;
            }

            if (cardVisualCatalog == null && cardBackSprite == null)
            {
                error = $"{name} is missing CardVisualCatalog or CardBackSprite.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
