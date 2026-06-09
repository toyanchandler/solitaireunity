using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class TransformUpdateProvider : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private TransformUpdateProviderData _data;

        #endregion

        #region Private Variables

        private Tween _tween;

        private Sequence _sequence;

        private Coroutine _moveInZCoroutine;

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            StopMoveInZCoroutine();
            _sequence?.Kill();
            _tween?.Kill();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Changes the position of the game object.
        /// </summary>
        protected virtual void ChangePosition()
        {
            StopMoveInZCoroutine();
            // Terminate any running sequences.
            _sequence?.Kill();
    
            // Initialize the DOTween sequence.
            _sequence = DOTween.Sequence();

            // Calculate the target position.
            Vector3 targetPosition = CalculateTargetPosition();

            // Perform the move tween.
            _tween = transform.DOMove(targetPosition, _data._duration);
            _sequence.Append(_tween);

            // On sequence complete.
            _sequence.OnComplete(OnSequenceComplete);

            // If the object should be destroyed upon arrival.
            if (_data._destroyOnArrived)
            {
                _sequence.AppendCallback(() => Destroy(gameObject));
            }

            // Play the sequence.
            _sequence.Play();
        }

        /// <summary>
        /// Calculates the target position based on data.
        /// </summary>
        /// <returns>Target position</returns>
        private Vector3 CalculateTargetPosition()
        {
            Vector3 targetPosition = new Vector3();
            targetPosition.z = transform.position.z + 4;

            if (_data._onlyHorizontal)
            {
                targetPosition.x = _data.targetX;
            }
            else
            {
                targetPosition = _data._targetTransform.position;
            }

            targetPosition.y = transform.position.y;

            return targetPosition;
        }

        /// <summary>
        /// On sequence complete actions.
        /// </summary>
        private void OnSequenceComplete()
        {
            _tween = null;

            if (_data._moveInPositiveZAfterX && _data._onlyHorizontal)
            {
                _moveInZCoroutine = StartCoroutine(MoveInZCoroutine());
            }
        }

        private void StopMoveInZCoroutine()
        {
            if (_moveInZCoroutine == null) return;

            StopCoroutine(_moveInZCoroutine);
            _moveInZCoroutine = null;
        }

        /// <summary>
        /// Coroutine to move in Z-axis.
        /// </summary>
        private IEnumerator MoveInZCoroutine()
        {
            Vector3 movementVector = new Vector3(0, 0, 0.1f);

            while (true)
            {
                transform.position += movementVector;
                yield return null;
            }
        }

        #endregion

    }
    
    [Serializable]
    public struct TransformUpdateProviderData
    {
        public Transform _targetTransform;
        public float targetX;
        public float _duration;
        public bool _destroyOnArrived;
        public bool _onlyHorizontal;
        public bool _moveInPositiveZAfterX;
    }
}
