using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [RequireComponent(typeof(CardMotionPresenter))]
    public sealed partial class CardView : MonoBehaviour
    {
        [SerializeField] private CardRuntimeIdentity identity;
        [SerializeField] private CardVisualStateMachine visualStateMachine;
        [SerializeField] private SpriteRenderer cardRenderer;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private SpriteRenderer dragShadowRenderer;
        [SerializeField] private SpriteRenderer selectionHighlightRenderer;
        [SerializeField] private CardMotionPresenter motionPresenter;

        private CardDragBehaviour _dragBehaviour;
        private Vector3 _homeScale;
        private Color _homeColor = Color.white;
        private bool _isDragVisualActive;
        private bool _isSelectionHighlightActive;

        public int CardId => CardViewLogic.Identity.ResolveCardId(identity);
        public Transform CachedTransform => transform;
        public bool IsPresenting => motionPresenter != null && motionPresenter.IsPresenting;
        public bool IsCardRendererVisible => CardViewLogic.MainRenderer.IsVisible(cardRenderer);

        private void Awake()
        {
            ResolveRequiredComponents();
            CacheHomeTransformState();
            motionPresenter.Initialize(this, visualStateMachine, _homeScale);
            EnsureDragShadow();
            EnsureSelectionHighlight();
            SetSelectionHighlight(false);
        }

        private void OnEnable()
        {
            if (!CardViewLogic.Guard.ShouldRegisterCard(CardId))
                return;

            SolitaireFeatureRegistration.RegisterCard(this);
        }

        private void OnDestroy() => SolitaireFeatureRegistration.UnregisterCard(this);

        public void Refresh(CardState state, SolitaireDeckConfigSO config)
        {
            if (CardViewLogic.Guard.ShouldSkipRefresh(IsPresenting))
                return;

            SyncIdentity(state);
            ApplyFaceSprites(state, config, state.IsFaceUp);
            visualStateMachine?.SetState(CardViewLogic.VisualState.ResolveIdleState(state.IsFaceUp));
        }

        public void ApplyBackFace(CardState state, SolitaireDeckConfigSO config)
        {
            SyncIdentity(state);
            ApplyFaceSprites(state, config, false);
            visualStateMachine?.SetState(CardVisualState.FaceDown);
        }

        public void ApplyLayoutSize(Vector2 worldSize)
        {
            if (!CardViewLogic.Layout.HasRenderableSprite(cardRenderer))
                return;

            if (!CardViewLogic.Layout.TryResolveUniformScale(
                    worldSize,
                    cardRenderer.sprite.bounds.size,
                    out float scale))
                return;

            ApplyResolvedLayoutScale(worldSize, scale);
        }

        public void SetSelectionHighlight(bool isActive)
        {
            EnsureSelectionHighlight();
            _isSelectionHighlightActive = isActive;
            CardViewLogic.SelectionHighlight.ApplyActiveState(selectionHighlightRenderer, isActive);
        }

        public void SetSortingOrder(int order)
        {
            DisableSortingGroup();
            ApplySortingOrders(new CardViewLogic.Sorting.ApplyValues(order));
        }

        public void SetCardRendererVisible(bool isVisible)
        {
            CardViewLogic.SpriteRendererOps.SetEnabled(cardRenderer, isVisible);

            if (!CardViewLogic.Visibility.ShouldClearVisuals(isVisible))
                return;

            SetSelectionHighlight(false);
            ResetDragShadow();
        }

        public void MoveTo(Vector3 targetPosition, float duration) =>
            motionPresenter.MoveTo(targetPosition, duration);

        public void PlayFlipReveal(CardState state, SolitaireDeckConfigSO config, float duration) =>
            motionPresenter.PlayFlipReveal(state, config, duration);

        public void PlayMoveThenFlip(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight = 0f) =>
            motionPresenter.PlayMoveThenFlip(targetPosition, state, config, moveDuration, flipDuration, arcHeight);

        public void PlayDealMove(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight,
            bool flipOnLand)
        {
            SetCardRendererVisible(true);
            motionPresenter.PlayDealMove(targetPosition, state, config, moveDuration, flipDuration, arcHeight, flipOnLand);
        }

        public void PlayWinPop(float height, float duration) => motionPresenter.PlayWinPop(height, duration);

        public void PlayInvalidFeedback() =>
            PlayInvalidFeedback(CardViewLogic.Constants.DefaultInvalidFeedbackDuration);

        public void PlayInvalidFeedback(float duration) => motionPresenter.PlayInvalidFeedback(duration);

        public void ApplyFaceSprites(CardState state, SolitaireDeckConfigSO config, bool isFaceUp)
        {
            motionPresenter.SetLastRenderedFaceUp(isFaceUp);
            cardRenderer.sprite = CardViewLogic.Sprites.ResolveFaceSprite(config, state, isFaceUp);
            SyncDragShadowSprite();
        }

        public bool Validate(out string error) =>
            CardViewLogic.Validation.TryValidate(name, identity, cardRenderer, motionPresenter, out error);
    }
}
