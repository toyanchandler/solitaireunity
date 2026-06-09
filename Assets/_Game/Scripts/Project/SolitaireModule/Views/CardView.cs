using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [RequireComponent(typeof(CardMotionPresenter))]
    public sealed class CardView : MonoBehaviour
    {
        [SerializeField] private CardRuntimeIdentity identity;
        [SerializeField] private CardVisualStateMachine visualStateMachine;
        [SerializeField] private SpriteRenderer cardRenderer;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private SpriteRenderer dragShadowRenderer;
        [SerializeField] private SpriteRenderer selectionHighlightRenderer;
        [SerializeField] private CardMotionPresenter motionPresenter;

        private static readonly Color SelectionHighlightColor = new Color(0.35f, 0.72f, 1f, 1f);

        private Vector3 _homeScale;
        private Color _homeColor = Color.white;
        private bool _isDragVisualActive;
        private bool _isSelectionHighlightActive;

        public int CardId => identity != null ? identity.CardId : -1;
        public Transform CachedTransform => transform;
        public bool IsPresenting => motionPresenter != null && motionPresenter.IsPresenting;

        private void Awake()
        {
            if (motionPresenter == null)
                motionPresenter = GetComponent<CardMotionPresenter>();

            if (motionPresenter == null)
                throw new System.InvalidOperationException($"{name} is missing CardMotionPresenter.");

            _homeScale = transform.localScale;

            if (cardRenderer != null)
                _homeColor = cardRenderer.color;

            motionPresenter.Initialize(this, visualStateMachine, _homeScale);
            EnsureDragShadow();
            EnsureSelectionHighlight();
            SetSelectionHighlight(false);
        }

        private void OnEnable()
        {
            if (CardId >= 0)
                SolitaireFeatureRegistration.RegisterCard(this);
        }

        private void OnDestroy()
        {
            SolitaireFeatureRegistration.UnregisterCard(this);
        }

        public void Refresh(CardState state, SolitaireDeckConfigSO config)
        {
            if (IsPresenting)
                return;

            if (identity != null && identity.CardId != state.Id)
                identity.SetIdentity(state.Id);

            ApplyFaceSprites(state, config, state.IsFaceUp);
            visualStateMachine?.SetState(state.IsFaceUp ? CardVisualState.FaceUpIdle : CardVisualState.FaceDown);
        }

        public void ApplyBackFace(CardState state, SolitaireDeckConfigSO config)
        {
            if (identity != null && identity.CardId != state.Id)
                identity.SetIdentity(state.Id);

            ApplyFaceSprites(state, config, false);
            visualStateMachine?.SetState(CardVisualState.FaceDown);
        }

        public void ApplyLayoutSize(Vector2 worldSize)
        {
            if (cardRenderer == null || cardRenderer.sprite == null)
                return;

            Vector2 spriteSize = cardRenderer.sprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            float scale = Mathf.Min(worldSize.x / spriteSize.x, worldSize.y / spriteSize.y);
            _homeScale = new Vector3(scale, scale, transform.localScale.z);
            transform.localScale = _homeScale;
            motionPresenter.SetHomeScale(_homeScale);

            if (TryGetComponent(out BoxCollider2D cardCollider))
                cardCollider.size = new Vector2(worldSize.x / scale, worldSize.y / scale);

            ApplySelectionHighlightSize(worldSize, scale);
        }

        public void SetSelectionHighlight(bool isActive)
        {
            EnsureSelectionHighlight();
            _isSelectionHighlightActive = isActive;

            if (selectionHighlightRenderer == null)
                return;

            selectionHighlightRenderer.enabled = isActive;

            if (isActive)
                selectionHighlightRenderer.color = SelectionHighlightColor;
        }

        public void SetSortingOrder(int order)
        {
            if (sortingGroup != null)
                sortingGroup.sortingOrder = order;
            else if (cardRenderer != null)
                cardRenderer.sortingOrder = order;
        }

        public void MoveTo(Vector3 targetPosition, float duration)
        {
            motionPresenter.MoveTo(targetPosition, duration);
        }

        public void PlayFlipReveal(CardState state, SolitaireDeckConfigSO config, float duration)
        {
            motionPresenter.PlayFlipReveal(state, config, duration);
        }

        public void PlayMoveThenFlip(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight = 0f)
        {
            motionPresenter.PlayMoveThenFlip(targetPosition, state, config, moveDuration, flipDuration, arcHeight);
        }

        public void PlayDealMove(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight,
            bool flipOnLand)
        {
            motionPresenter.PlayDealMove(targetPosition, state, config, moveDuration, flipDuration, arcHeight, flipOnLand);
        }

        public void BeginDragVisual(SolitaireDeckConfigSO config)
        {
            if (config == null || IsPresenting)
                return;

            SetSelectionHighlight(true);
            _isDragVisualActive = true;
            visualStateMachine?.SetState(CardVisualState.Dragging);
            transform.localScale = _homeScale * config.DragLiftScale;

            if (cardRenderer != null)
            {
                Color color = _homeColor;
                color.a = config.DragAlpha;
                cardRenderer.color = color;
            }

            if (dragShadowRenderer != null)
            {
                if (dragShadowRenderer.sprite == null && cardRenderer != null)
                    dragShadowRenderer.sprite = cardRenderer.sprite;

                dragShadowRenderer.transform.localPosition = new Vector3(
                    config.DragShadowOffset.x,
                    config.DragShadowOffset.y,
                    0.01f);
                Color shadowColor = Color.black;
                shadowColor.a = config.DragShadowAlpha;
                dragShadowRenderer.color = shadowColor;
            }
        }

        public void EndDragVisual()
        {
            _isDragVisualActive = false;
            SetSelectionHighlight(false);
            ResetFeedback();
            ResetDragShadow();
        }

        public void PlayPressedFeedback()
        {
            if (IsPresenting || _isDragVisualActive)
                return;

            transform.localScale = _homeScale * 1.04f;
        }

        public void ResetFeedback()
        {
            if (IsPresenting)
                return;

            transform.localScale = _homeScale;

            if (cardRenderer != null)
                cardRenderer.color = _homeColor;
        }

        public void PlayWinPop(float height, float duration)
        {
            motionPresenter.PlayWinPop(height, duration);
        }

        public void PlayInvalidFeedback()
        {
            PlayInvalidFeedback(0.12f);
        }

        public void PlayInvalidFeedback(float duration)
        {
            motionPresenter.PlayInvalidFeedback(duration);
        }

        public void ApplyFaceSprites(CardState state, SolitaireDeckConfigSO config, bool isFaceUp)
        {
            motionPresenter.SetLastRenderedFaceUp(isFaceUp);
            cardRenderer.sprite = isFaceUp ? config.GetCardFrontSprite(state) : config.GetCardBackSprite(state);

            if (dragShadowRenderer != null && _isDragVisualActive && dragShadowRenderer.sprite == null)
                dragShadowRenderer.sprite = cardRenderer.sprite;
        }

        public bool Validate(out string error)
        {
            if (identity == null)
            {
                error = $"{name} is missing CardRuntimeIdentity.";
                return false;
            }

            if (cardRenderer == null)
            {
                error = $"{name} is missing SpriteRenderer.";
                return false;
            }

            if (motionPresenter == null)
            {
                error = $"{name} is missing CardMotionPresenter.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void EnsureDragShadow()
        {
            if (dragShadowRenderer != null)
                return;

            Transform existing = transform.Find("DragShadow");

            if (existing != null)
            {
                dragShadowRenderer = existing.GetComponent<SpriteRenderer>();
                return;
            }

            var shadowObject = new GameObject("DragShadow");
            shadowObject.transform.SetParent(transform, false);
            shadowObject.transform.localPosition = new Vector3(0.04f, -0.05f, 0.01f);
            dragShadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            dragShadowRenderer.sortingOrder = -1;

            if (cardRenderer != null)
            {
                dragShadowRenderer.sprite = cardRenderer.sprite;
                dragShadowRenderer.sortingLayerID = cardRenderer.sortingLayerID;
            }

            ResetDragShadow();
        }

        private void ResetDragShadow()
        {
            if (dragShadowRenderer == null)
                return;

            Color shadowColor = dragShadowRenderer.color;
            shadowColor.a = 0f;
            dragShadowRenderer.color = shadowColor;
        }

        private void EnsureSelectionHighlight()
        {
            if (selectionHighlightRenderer != null)
                return;

            Transform existing = transform.Find("SelectionHighlight");

            if (existing != null)
            {
                selectionHighlightRenderer = existing.GetComponent<SpriteRenderer>();
                selectionHighlightRenderer.enabled = _isSelectionHighlightActive;
                return;
            }

            var highlightObject = new GameObject("SelectionHighlight");
            highlightObject.transform.SetParent(transform, false);
            highlightObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            selectionHighlightRenderer = highlightObject.AddComponent<SpriteRenderer>();
            selectionHighlightRenderer.sortingOrder = 2;
            selectionHighlightRenderer.enabled = false;

            if (cardRenderer != null)
            {
                selectionHighlightRenderer.sortingLayerID = cardRenderer.sortingLayerID;

                if (selectionHighlightRenderer.sprite == null)
                    selectionHighlightRenderer.sprite = Resources.Load<Sprite>("Solitaire/card_selection_highlight");
            }
        }

        private void ApplySelectionHighlightSize(Vector2 worldSize, float cardScale)
        {
            if (selectionHighlightRenderer == null || selectionHighlightRenderer.sprite == null)
                return;

            Vector2 spriteSize = selectionHighlightRenderer.sprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            float highlightScale = Mathf.Min(worldSize.x / (spriteSize.x * cardScale), worldSize.y / (spriteSize.y * cardScale));
            selectionHighlightRenderer.transform.localScale = new Vector3(highlightScale, highlightScale, 1f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            identity = GetComponent<CardRuntimeIdentity>();
            visualStateMachine = GetComponent<CardVisualStateMachine>();
            cardRenderer = GetComponent<SpriteRenderer>();
            sortingGroup = GetComponent<SortingGroup>();
            motionPresenter = GetComponent<CardMotionPresenter>();

            Transform dragShadow = transform.Find("DragShadow");

            if (dragShadow != null)
                dragShadowRenderer = dragShadow.GetComponent<SpriteRenderer>();

            Transform selectionHighlight = transform.Find("SelectionHighlight");

            if (selectionHighlight != null)
                selectionHighlightRenderer = selectionHighlight.GetComponent<SpriteRenderer>();
        }
#endif
    }
}
