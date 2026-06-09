using System;
using System.Collections.Generic;
using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Stacking
{
    public class EntityStackController : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private EntityStackControllerData entityStackControllerData;

        [SerializeField] private StackDataSO stackDataSO;
        
        [SerializeField] private Vector2 _randomOffsetRange = new Vector2(1.0f, 1.0f);
        
        [SerializeField] private GameObject _damageEffectPrefab;
        
        [SerializeField] private float _effectDuration = 2.0f;

        #endregion

        #region Private Variables

        private CoroutineService _coroutineService;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            entityStackControllerData.StackableObjects = new Stack<GameObject>();
            _coroutineService = new CoroutineService(this);

            if (entityStackControllerData.StackPosition == null)
                entityStackControllerData.StackPosition = transform;
        }

        private void OnEnable()
        {
            EventManager.StackableEvents.Stack += AddToStack;
            EventManager.StackableEvents.Unstack += RemoveFromStack;
            EventManager.InGameEvents.LevelStart += StartCoroutine;
            EventManager.InGameEvents.LevelLoaded += ResetEntityStackControllerData;
        }

        private void OnDisable()
        {
            EventManager.StackableEvents.Stack -= AddToStack;
            EventManager.StackableEvents.Unstack -= RemoveFromStack;
            EventManager.InGameEvents.LevelStart -= StartCoroutine;
            EventManager.InGameEvents.LevelLoaded -= ResetEntityStackControllerData;
        }

        #endregion

        #region Public Methods

        [Button]
        public void CallUnstackEvent(StackableData stackableData)
        {
            EventManager.StackableEvents.Unstack?.Invoke(stackableData);
        }

        #endregion

        #region Private Methods

        private void StartCoroutine()
        {
            _coroutineService.StartUpdateRoutine(SmoothFollowStackedObjects, () => true);
        }

        #region Private Methods

        private void AddToStack(StackableData stackableData)
        {
            RegisterStackable(stackableData);
            SetStackableObjectPosition(stackableData);
            PushToStack(stackableData.Object);
            ApplyOceanWaveEffect(stackableData);
        }


        private void RemoveFromStack(StackableData stackableData)
        {
            if (IsStackEmpty()) return;

            UnregisterStackable(stackableData);
            var poppedObject = PopFromStack();
            Destroy(poppedObject);
        }

        private void SmoothFollowStackedObjects()
        {
            if (IsStackEmpty()) return;

            var stackableObjects = GetStackArray();
            var targetPosition = GetStackTargetPosition();

            for (var i = stackableObjects.Length - 1; i >= 0; i--) SmoothFollow(stackableObjects, i, targetPosition);
        }

        #endregion

        private void RegisterStackable(StackableData stackableData)
        {
            stackDataSO.RegisterStackableData(stackableData.Type, stackableData.Amount);
            entityStackControllerData._stackCount++;
        }

        private void UnregisterStackable(StackableData stackableData)
        {
            stackDataSO.UnregisterStackableData(stackableData.Type, stackableData.Amount);
            entityStackControllerData._stackCount--;
        }

        private void SetStackableObjectPosition(StackableData stackableData)
        {
            var newPosition = CalculateNewPosition(stackableData.Object.transform.position);
            stackableData.Object.transform.position = newPosition;
        }

        private Vector3 CalculateNewPosition(Vector3 currentPosition)
        {
            switch (entityStackControllerData.stackDirection)
            {
                case EntityStackControllerData.StackDirection.Y:
                    return currentPosition + new Vector3(0,
                        entityStackControllerData._stackCount * entityStackControllerData.StackStepY, 0);
                case EntityStackControllerData.StackDirection.Z:
                    return currentPosition + new Vector3(0, 0,
                        entityStackControllerData._stackCount * -entityStackControllerData.StackStepZ);
                default:
                    return currentPosition;
            }
        }

        private void PushToStack(GameObject stackableObject)
        {
            entityStackControllerData.StackableObjects.Push(stackableObject);
        }

        private GameObject PopFromStack()
        {
            return entityStackControllerData.StackableObjects.Pop();
        }

        private bool IsStackEmpty()
        {
            return entityStackControllerData.StackableObjects.Count == 0;
        }

        private GameObject[] GetStackArray()
        {
            return entityStackControllerData.StackableObjects.ToArray();
        }

        private Vector3 GetStackTargetPosition()
        {
            return entityStackControllerData.StackPosition.position;
        }

        private void SmoothFollow(GameObject[] stackableObjects, int i, Vector3 targetPosition)
        {
            var followTarget = i == stackableObjects.Length - 1
                ? targetPosition
                : stackableObjects[i + 1].transform.position;
            var currentPosition = stackableObjects[i].transform.position;

            switch (entityStackControllerData.stackDirection)
            {
                case EntityStackControllerData.StackDirection.Y:
                    FollowInYDirection(stackableObjects[i], followTarget, currentPosition);
                    break;
                case EntityStackControllerData.StackDirection.Z:
                    FollowInZDirection(stackableObjects[i], followTarget, currentPosition);
                    break;
            }
        }

        private void FollowInYDirection(GameObject stackableObject, Vector3 followTarget, Vector3 currentPosition)
        {
            stackableObject.transform.position = new Vector3(
                Mathf.Lerp(currentPosition.x, followTarget.x,
                    Time.smoothDeltaTime * entityStackControllerData.smoothFollowSpeed),
                currentPosition.y,
                Mathf.Lerp(currentPosition.z, followTarget.z, Time.deltaTime * 500)
            );
        }

        private void FollowInZDirection(GameObject stackableObject, Vector3 followTarget, Vector3 currentPosition)
        {
            stackableObject.transform.position = new Vector3(
                Mathf.Lerp(currentPosition.x, followTarget.x, Time.smoothDeltaTime * entityStackControllerData.smoothFollowSpeed),
                Mathf.Lerp(currentPosition.y, followTarget.y, Time.smoothDeltaTime * entityStackControllerData.smoothFollowSpeed),
                Mathf.Lerp(currentPosition.z, followTarget.z - entityStackControllerData.StackStepZ + 0.25f, Time.smoothDeltaTime * entityStackControllerData.smoothFollowSpeed)
            );
        }
        
        private void ApplyOceanWaveEffect(StackableData stackableData)
        {
            GameObject[] stackableObjects = GetStackArray();
            Sequence sequence = DOTween.Sequence();

            float initialDelay = 0.1f;  // Initial delay before the first animation starts
            float delayBetween = 0.05f; // Delay between each object's animation

            for (int i = 0; i < stackableObjects.Length; i++)
            {
                GameObject stackable = stackableObjects[i];

                // Calculate the total delay for this object
                float totalDelay = initialDelay + (i * delayBetween);

                // Create a local sequence for each stackable
                Sequence localSeq = DOTween.Sequence();

                // Apply punch scale with easing
                localSeq.Append(
                    stackable.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.1f)
                        .SetEase(Ease.Linear)
                );

                // Return to original scale with easing
                localSeq.Append(
                    stackable.transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f)
                        .SetEase(Ease.Linear)
                );

                // If it's the last object, add a callback to create the curvy movement effect
                if (i == stackableObjects.Length - 1)
                {
                    localSeq.OnComplete(() => 
                    {
                        for (int j = 0; j < stackableData.Amount; j++)
                        {
                            GameObject damageEffectInstance = Instantiate(_damageEffectPrefab, stackable.transform.position, Quaternion.identity);
                            CreateCurvyMovement(damageEffectInstance, stackable.transform);
                        }
                    });
                }

                // Add the local sequence to the main sequence with a delay
                sequence.Insert(totalDelay, localSeq);
            }

            // Play the main sequence
            sequence.Play();
        }

        
        private void CreateCurvyMovement(GameObject damageEffect, Transform stackable)
        {
            damageEffect.GetComponent<Collider>().enabled = false;
            var position = damageEffect.transform.position;
            Vector3 endPosition = position + new Vector3(Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x), 0, 7);
            endPosition.y = 0;

            // Define a midpoint for the curve. Higher the Y value, more pronounced the curve.
            Vector3 midPoint = (position + endPosition) / 2 + Vector3.up * 2.5f;

            Vector3[] path = new Vector3[] { midPoint, endPosition };

            damageEffect.transform.DOLocalPath(path, _effectDuration, PathType.CatmullRom).OnComplete(() =>
                {
                    damageEffect.GetComponent<Collider>().enabled = true;
                })
                .SetOptions(false)
                .SetEase(Ease.InOutQuad);
        }

        private void ResetEntityStackControllerData(GameObject arg0)
        {
            entityStackControllerData._stackCount = 0;
            entityStackControllerData.StackableObjects.Clear();
        }

        #endregion

        #region Data Structures

        [Serializable]
        internal struct EntityStackControllerData
        {
            public Transform StackPosition;
            public Stack<GameObject> StackableObjects;
            [ReadOnly] public int _stackCount;
            public float smoothFollowSpeed;
            public StackDirection stackDirection;
            
            [ShowIf("@stackDirection == StackDirection.Y" )] public float StackStepY;
            [ShowIf("@stackDirection == StackDirection.Z")] public float StackStepZ;

            [EnumToggleButtons] [GUIColor(0.3f, 0.8f, 0.8f)]
            public enum StackDirection
            {
                Y,
                Z
            }
        }
        
        #endregion
    }
}
