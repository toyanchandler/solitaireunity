using System.Collections;
using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [DisallowMultipleComponent]
    public sealed class CardMotionPresenter : MonoBehaviour
    {
        private CardView _owner;
        private CardVisualStateMachine _visualStateMachine;
        private Coroutine _presentationRoutine;
        private Vector3 _homeScale;
        private Quaternion _homeRotation;
        private bool _lastRenderedFaceUp;

        public bool IsPresenting => _presentationRoutine != null;

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
            StartPresentation(MoveToRoutine(targetPosition, duration, 0f));
        }

        public void PlayFlipReveal(CardState state, SolitaireDeckConfigSO config, float duration)
        {
            StartPresentation(FlipRevealRoutine(state, config, duration, transform.position));
        }

        public void PlayMoveThenFlip(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight = 0f)
        {
            StartPresentation(MoveThenFlipRoutine(targetPosition, state, config, moveDuration, flipDuration, arcHeight));
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
            StartPresentation(DealMoveRoutine(targetPosition, moveDuration, arcHeight));
        }

        public void PlayWinPop(float height, float duration)
        {
            StartPresentation(WinPopRoutine(height, duration));
        }

        public void PlayInvalidFeedback(float duration)
        {
            StartPresentation(InvalidFeedbackRoutine(duration));
        }

        public void StopPresentation()
        {
            if (_presentationRoutine != null)
                StopCoroutine(_presentationRoutine);

            _presentationRoutine = null;
        }

        private void StartPresentation(IEnumerator routine)
        {
            if (_presentationRoutine != null)
                StopCoroutine(_presentationRoutine);

            _presentationRoutine = StartCoroutine(routine);
        }

        private IEnumerator MoveToRoutine(Vector3 targetPosition, float duration, float arcHeight)
        {
            _visualStateMachine?.SetState(CardVisualState.Moving);
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseOutCubic(t);
                Vector3 flat = Vector3.LerpUnclamped(start, targetPosition, eased);
                float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
                transform.position = flat + new Vector3(0f, arc, 0f);
                yield return null;
            }

            transform.position = targetPosition;
            _visualStateMachine?.SetState(_lastRenderedFaceUp ? CardVisualState.FaceUpIdle : CardVisualState.FaceDown);
            _presentationRoutine = null;
        }

        private IEnumerator DealMoveRoutine(Vector3 targetPosition, float duration, float arcHeight)
        {
            yield return MoveToRoutine(targetPosition, duration, arcHeight);
        }

        private IEnumerator MoveThenFlipRoutine(
            Vector3 targetPosition,
            CardState state,
            SolitaireDeckConfigSO config,
            float moveDuration,
            float flipDuration,
            float arcHeight)
        {
            _owner.ApplyBackFace(state, config);
            _visualStateMachine?.SetState(CardVisualState.Moving);
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                float eased = EaseOutCubic(t);
                Vector3 flat = Vector3.LerpUnclamped(start, targetPosition, eased);
                float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
                transform.position = flat + new Vector3(0f, arc, 0f);
                yield return null;
            }

            transform.position = targetPosition;
            yield return FlipRevealRoutine(state, config, flipDuration, targetPosition);
        }

        private IEnumerator FlipRevealRoutine(
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
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float eased = EaseInCubic(t);
                float scaleX = Mathf.Lerp(1f, 0.02f, eased);
                transform.localScale = new Vector3(_homeScale.x * scaleX, _homeScale.y, _homeScale.z);
                transform.position = anchorPosition + new Vector3(0f, lift * eased, 0f);
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, tilt, eased));
                yield return null;
            }

            _owner.ApplyFaceSprites(state, config, true);
            _lastRenderedFaceUp = true;

            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float eased = EaseOutCubic(t);
                float scaleX = Mathf.Lerp(0.02f, 1f, eased);
                transform.localScale = new Vector3(_homeScale.x * scaleX, _homeScale.y, _homeScale.z);
                transform.position = anchorPosition + new Vector3(0f, lift * (1f - eased), 0f);
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(tilt, 0f, eased));
                yield return null;
            }

            transform.localScale = _homeScale;
            transform.localRotation = _homeRotation;
            transform.position = anchorPosition;
            _visualStateMachine?.SetState(CardVisualState.FaceUpIdle);
            _presentationRoutine = null;
        }

        private IEnumerator InvalidFeedbackRoutine(float duration)
        {
            Vector3 start = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Sin(elapsed * 90f) * 0.035f;
                transform.localPosition = start + new Vector3(offset, 0f, 0f);
                yield return null;
            }

            transform.localPosition = start;
            _presentationRoutine = null;
        }

        private IEnumerator WinPopRoutine(float height, float duration)
        {
            _visualStateMachine?.SetState(CardVisualState.Moving);
            Vector3 start = transform.position;
            Vector3 peak = start + new Vector3(0f, height, 0f);
            float halfDuration = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float eased = EaseOutCubic(t);
                transform.position = Vector3.LerpUnclamped(start, peak, eased);
                transform.localScale = Vector3.LerpUnclamped(_homeScale, _homeScale * 1.08f, eased);
                yield return null;
            }

            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float eased = EaseInCubic(t);
                transform.position = Vector3.LerpUnclamped(peak, start, eased);
                transform.localScale = Vector3.LerpUnclamped(_homeScale * 1.08f, _homeScale, eased);
                yield return null;
            }

            transform.position = start;
            transform.localScale = _homeScale;
            _visualStateMachine?.SetState(CardVisualState.FaceUpIdle);
            _presentationRoutine = null;
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static float EaseInCubic(float t) => t * t * t;
    }
}
