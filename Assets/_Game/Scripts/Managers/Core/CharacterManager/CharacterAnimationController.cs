using _Game.Scripts.Helper.Extensions.System;
using Handler.Extensions;
using UnityEngine;
namespace _Game.Scripts.Managers.Core.CharacterManager
{
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterAnimationController : MonoBehaviour
    {
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int RunHash = Animator.StringToHash("Run");
        private static readonly int DanceHash = Animator.StringToHash("Dance");

        [SerializeField] private Animator _animator;
        
        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            ValidateReferences();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {
            EventManager.InGameEvents.LevelSuccess += DanceState;
            EventManager.InGameEvents.LevelFail += IdleState;
            EventManager.InGameEvents.LevelStart += RunState;
            EventManager.InGameEvents.LevelLoaded += IdleState;
        }

        private void UnsubscribeEvents()
        {
            EventManager.InGameEvents.LevelSuccess -= DanceState;
            EventManager.InGameEvents.LevelFail -= IdleState;
            EventManager.InGameEvents.LevelStart -= RunState;
            EventManager.InGameEvents.LevelLoaded -= IdleState;
        }

        private void Start()
        {
            IdleState();
        }
 
        private void RunState()
        {
            SetTrigger(RunHash);
        }

        private void IdleState(GameObject level)
        {
            IdleState();
        }

        private void IdleState()
        {
            SetTrigger(IdleHash);
            if (transform.parent != null)
            {
                transform.LookAt(transform.parent);
            }
        }

        private void DanceState()
        {
            SetTrigger(DanceHash);
        }

        private void SetTrigger(int triggerHash)
        {
            if (_animator == null)
            {
                TDebug.LogWarning($"{nameof(CharacterAnimationController)} cannot switch animation because Animator is missing.");
                return;
            }

            _animator.SetTrigger(triggerHash);
        }

        private void ValidateReferences()
        {
            if (_animator == null)
            {
                TDebug.LogWarning($"{nameof(CharacterAnimationController)} requires an Animator.");
            }
        }
    }
}
