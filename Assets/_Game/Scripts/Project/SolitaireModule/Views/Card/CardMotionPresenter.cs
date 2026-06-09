using _Game.Scripts.Project.SolitaireModule.Data;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [DisallowMultipleComponent]
    public sealed class CardMotionPresenter : MonoBehaviour
    {
        private CardView _owner;
        private CardVisualStateMachine _visualStateMachine;
        private Sequence _presentationSequence;
        private Vector3 _homeScale;
        private Quaternion _homeRotation;
        private bool _lastRenderedFaceUp;

        public bool IsPresenting => _presentationSequence != null && _presentationSequence.IsActive();

        public void Initialize(CardView owner, CardVisualStateMachine visualStateMachine, Vector3 homeScale)
        {
            _owner = owner;
            _visualStateMachine = visualStateMachine;
            _homeScale = homeScale;
            _homeRotation = transform.localRotation;
        }

        public void SetHomeScale(Vector3 homeScale)
        {
            _homeScale = homeScale;
        }

        public void SetLastRenderedFaceUp(bool isFaceUp)
        {
            _lastRenderedFaceUp = isFaceUp;
        }

        public void MoveTo(Vector3 targetPosition, float duration)
        {
            StartPresentation(BuildMoveToSequence(targetPosition, duration, 0f));
        }

        public void PlayFlipReveal(CardState state, SolitaireDeckConfigSO config, float duration)
        {
            StartPresentation(BuildFlipRevealSequence(state, config, duration, transform.position));
        }

        public void PlayMoveThenFlip(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight = 0f)
        {
            StartPresentation(BuildMoveThenFlipSequence(targetPosition, state, config, moveDuration, flipDuration, arcHeight));
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
            if (flipOnLand)
            {
                PlayMoveThenFlip(targetPosition, state, config, moveDuration, flipDuration, arcHeight);
                return;
            }

            _owner.ApplyBackFace(state, config);
            StartPresentation(BuildMoveToSequence(targetPosition, moveDuration, arcHeight));
        }

        public void PlayWinPop(float height, float duration)
        {
            StartPresentation(BuildWinPopSequence(height, duration));
        }

        public void PlayInvalidFeedback(float duration)
        {
            StartPresentation(BuildInvalidFeedbackSequence(duration));
        }

        public void StopPresentation()
        {
            if (_presentationSequence == null)
                return;

            _presentationSequence.Kill();
            _presentationSequence = null;
        }

        private void OnDestroy()
        {
            _presentationSequence?.Kill();
            _presentationSequence = null;
        }

        private void StartPresentation(Sequence sequence)
        {
            _presentationSequence?.Kill();
            _presentationSequence = sequence;
            sequence.OnComplete(() => _presentationSequence = null);
            sequence.Play();
        }

        private Sequence BuildMoveToSequence(Vector3 targetPosition, float duration, float arcHeight)
        {
            _visualStateMachine?.SetState(CardVisualState.Moving);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(CreateArcMoveTween(targetPosition, duration, arcHeight));
            sequence.AppendCallback(() =>
            {
                transform.position = targetPosition;
                _visualStateMachine?.SetState(_lastRenderedFaceUp ? CardVisualState.FaceUpIdle : CardVisualState.FaceDown);
            });

            return sequence;
        }

        private Sequence BuildMoveThenFlipSequence(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight)
        {
            _owner.ApplyBackFace(state, config);
            _visualStateMachine?.SetState(CardVisualState.Moving);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(CreateArcMoveTween(targetPosition, moveDuration, arcHeight));
            sequence.AppendCallback(() => transform.position = targetPosition);
            sequence.Append(BuildFlipRevealSequence(state, config, flipDuration, targetPosition));

            return sequence;
        }

        private Sequence BuildFlipRevealSequence(
            CardState state,
            SolitaireDeckConfigSO config,
            float duration,
            Vector3 anchorPosition)
        {
            _visualStateMachine?.SetState(CardVisualState.Moving);
            _owner.ApplyBackFace(state, config);

            float halfDuration = duration * 0.5f;
            float lift = config.FlipLiftHeight;
            float tilt = config.FlipTiltDegrees;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(CreateFlipHalfTween(anchorPosition, lift, tilt, halfDuration, true));
            sequence.AppendCallback(() =>
            {
                _owner.ApplyFaceSprites(state, config, true);
                _lastRenderedFaceUp = true;
            });
            sequence.Append(CreateFlipHalfTween(anchorPosition, lift, tilt, halfDuration, false));
            sequence.AppendCallback(() =>
            {
                transform.localScale = _homeScale;
                transform.localRotation = _homeRotation;
                transform.position = anchorPosition;
                _visualStateMachine?.SetState(CardVisualState.FaceUpIdle);
            });

            return sequence;
        }

        private Sequence BuildInvalidFeedbackSequence(float duration)
        {
            Vector3 start = transform.localPosition;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                DOTween.To(() => 0f, elapsed =>
                {
                    float offset = Mathf.Sin(elapsed * 90f) * 0.035f;
                    transform.localPosition = start + new Vector3(offset, 0f, 0f);
                }, duration, duration));
            sequence.AppendCallback(() => transform.localPosition = start);

            return sequence;
        }

        private Sequence BuildWinPopSequence(float height, float duration)
        {
            _visualStateMachine?.SetState(CardVisualState.Moving);

            Vector3 start = transform.position;
            Vector3 peak = start + new Vector3(0f, height, 0f);
            Vector3 popScale = _homeScale * 1.08f;
            float halfDuration = duration * 0.5f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(CreateWinPopHalfTween(start, peak, _homeScale, popScale, halfDuration, true));
            sequence.Append(CreateWinPopHalfTween(peak, start, popScale, _homeScale, halfDuration, false));
            sequence.AppendCallback(() =>
            {
                transform.position = start;
                transform.localScale = _homeScale;
                _visualStateMachine?.SetState(CardVisualState.FaceUpIdle);
            });

            return sequence;
        }

        private Tween CreateArcMoveTween(Vector3 targetPosition, float duration, float arcHeight)
        {
            Vector3 start = transform.position;

            return DOTween.To(() => 0f, t =>
                {
                    Vector3 flat = Vector3.LerpUnclamped(start, targetPosition, t);
                    float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
                    transform.position = flat + new Vector3(0f, arc, 0f);
                }, 1f, duration)
                .SetEase(Ease.OutCubic);
        }

        private Tween CreateFlipHalfTween(
            Vector3 anchorPosition,
            float lift,
            float tilt,
            float duration,
            bool closing)
        {
            return DOTween.To(() => 0f, t =>
                {
                    float scaleX = closing ? Mathf.Lerp(1f, 0.02f, t) : Mathf.Lerp(0.02f, 1f, t);
                    transform.localScale = new Vector3(_homeScale.x * scaleX, _homeScale.y, _homeScale.z);
                    transform.position = anchorPosition + new Vector3(0f, closing ? lift * t : lift * (1f - t), 0f);
                    transform.localRotation = Quaternion.Euler(
                        0f,
                        0f,
                        closing ? Mathf.Lerp(0f, tilt, t) : Mathf.Lerp(tilt, 0f, t));
                }, 1f, duration)
                .SetEase(closing ? Ease.InCubic : Ease.OutCubic);
        }

        private Tween CreateWinPopHalfTween(
            Vector3 from,
            Vector3 to,
            Vector3 fromScale,
            Vector3 toScale,
            float duration,
            bool rising)
        {
            return DOTween.To(() => 0f, t =>
                {
                    transform.position = Vector3.LerpUnclamped(from, to, t);
                    transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
                }, 1f, duration)
                .SetEase(rising ? Ease.OutCubic : Ease.InCubic);
        }
    }
}
