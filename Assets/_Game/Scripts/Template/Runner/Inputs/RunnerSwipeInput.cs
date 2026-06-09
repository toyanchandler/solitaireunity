using System;
using System.Collections.Generic;
using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using DG.Tweening;
using Handler.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public class RunnerSwipeInput : MonoBehaviour
    {
        #region Inspector Fields

        public PathSettings pathSettings;

        #endregion

        #region Private Variables

        private Vector2 _startTouch;
        private Vector2 _swipeDelta;

        private CoroutineService _coroutineService;
        private Coroutine _swipeCoroutine;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _coroutineService = new CoroutineService(this);
            
            for (var i = 0; i < pathSettings.numberOfPaths; i++)
            {
                pathSettings._pathPositions.Add(i);
            }
        }

        #endregion

        #region Event Subscription

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelLoaded += SetInitialCurrentPathIndex;
            EventManager.InGameEvents.LevelLoaded += CalculatePathPositions;
            EventManager.InGameEvents.LevelStart += StartInput;
            EventManager.InGameEvents.LevelStart += StartCoroutine;
            EventManager.InGameEvents.EndMetaStart += StopCoroutine;
            EventManager.InGameEvents.LevelFail += StopCoroutine;
            EventManager.InGameEvents.LevelSuccess += StopCoroutine;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelLoaded -= SetInitialCurrentPathIndex;
            EventManager.InGameEvents.LevelLoaded -= CalculatePathPositions;
            EventManager.InGameEvents.LevelStart -= StartInput;
            EventManager.InGameEvents.LevelStart -= StartCoroutine;
            EventManager.InGameEvents.EndMetaStart += StopCoroutine;
            EventManager.InGameEvents.LevelFail -= StopCoroutine;
            EventManager.InGameEvents.LevelSuccess -= StopCoroutine;
        }

        #endregion

        #region Input Handling

        private void StartCoroutine() => _swipeCoroutine = _coroutineService.StartUpdateRoutine(HandleSwipeInput, () => true);

        private void StopCoroutine() => _coroutineService.Stop(_swipeCoroutine);

        private void StartInput()
        {
            NotifyPathChanged();
        }

        private void HandleSwipeInput()
        {
            CaptureInitialTouch();
            CaptureSwipeCompletion();
            CalculateSwipeDistance();
            HandleSwipeDirection();
        }

        private void SetInitialCurrentPathIndex(GameObject level)
        {
            if (pathSettings.initialPathIndex >= 0 && pathSettings.initialPathIndex < pathSettings.numberOfPaths)
            {
                pathSettings._currentPathIndex = pathSettings.initialPathIndex;
            }
            else
            {
                Debug.LogWarning("Invalid initial current path index. It should be between 0 and " + (pathSettings.numberOfPaths - 1));
            }
        }

        private void CaptureInitialTouch()
        {
            if (Input.GetMouseButtonDown(0) || (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began))
            {
                _startTouch = Input.touches.Length > 0 ? Input.touches[0].position : Input.mousePosition;
            }
        }

        private void CaptureSwipeCompletion()
        {
            if (Input.GetMouseButtonUp(0) || (Input.touches.Length > 0 && (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)))
            {
                _startTouch = _swipeDelta = Vector2.zero;
            }
        }

        private void CalculateSwipeDistance()
        {
            _swipeDelta = Vector2.zero;
            if (_startTouch != Vector2.zero)
                _swipeDelta = Input.touches.Length > 0 ? Input.touches[0].position - _startTouch : (Vector2)Input.mousePosition - _startTouch;
        }

        private void HandleSwipeDirection()
        {
            if (_swipeDelta.magnitude > 50)
            {
                var x = _swipeDelta.x;

                if (x < 0 && pathSettings._currentPathIndex > 0)
                    MoveCharacter(pathSettings._currentPathIndex - 1);
                else if (x > 0 && pathSettings._currentPathIndex < pathSettings.numberOfPaths - 1)
                    MoveCharacter(pathSettings._currentPathIndex + 1);

                _startTouch = _swipeDelta = Vector2.zero;
            }
        }

        #endregion

        #region Path Calculations

        private void CalculatePathPositions(GameObject go)
        {
            pathSettings._pathPositions.Clear();

            float step = (pathSettings.pathOffset.y - pathSettings.pathOffset.x) / (pathSettings.numberOfPaths - 1);
            for (int i = 0; i < pathSettings.numberOfPaths; i++)
            {
                pathSettings._pathPositions.Add(pathSettings.pathOffset.x + step * i);
            }
        }

        private void MoveCharacter(int targetPathIndex)
        {
            float targetX = pathSettings._pathPositions[targetPathIndex];

            var mySequence = DOTween.Sequence();
            mySequence.Append(transform.DOMoveX(targetX, 0.15f));
            mySequence.OnComplete(() =>
            {
                pathSettings._currentPathIndex = targetPathIndex;
                NotifyPathChanged();
            });
        }

        #endregion

        #region Path Notification

        private void NotifyPathChanged()
        {
            EventManager.PathEvents.PathChanged?.Invoke(pathSettings._currentPathIndex);
        }

        #endregion
    }
    
    [Serializable]
    public struct PathSettings
    {
        public int numberOfPaths;
        public Vector2 pathOffset;
        public int initialPathIndex;
        
        public List<float> _pathPositions;
        [ReadOnly] public int _currentPathIndex;
    }
}
