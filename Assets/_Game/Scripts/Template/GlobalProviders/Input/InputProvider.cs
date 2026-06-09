using System;
using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Input
{
    public abstract class InputProvider : MonoBehaviour
    {
        #region Private Variables

        private ClickData _clickData;
        
        private Coroutine _clickableCoroutine;
        
        private CoroutineService _coroutineService;

        #endregion
        
        #region Unity Methods

        private void Awake() => Initialize();

        protected virtual void Initialize()
        {
            _coroutineService = new CoroutineService(this);
        }
        
        protected virtual void OnEnable()
        {
            EventManager.ClickableEvents.ClickDown += OnClickDown;
            EventManager.ClickableEvents.ClickHold += OnClickHold;
            EventManager.ClickableEvents.ClickUp += OnClickUp;
            
            EventManager.InGameEvents.LevelStart += StartClickableEvents;
        }

        protected virtual void OnDisable()
        {
            EventManager.ClickableEvents.ClickDown -= OnClickDown;
            EventManager.ClickableEvents.ClickHold -= OnClickHold;
            EventManager.ClickableEvents.ClickUp -= OnClickUp;
            
            EventManager.InGameEvents.LevelStart -= StartClickableEvents;
        }

        #endregion

        #region Private Methods

        private void StartClickableEvents() => _clickableCoroutine = _coroutineService.StartUpdateRoutine(ClickableEvents, () => true);
        
        private void ClickableEvents()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                EventManager.ClickableEvents.ClickDown?.Invoke(_clickData);
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                EventManager.ClickableEvents.ClickHold?.Invoke(_clickData);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EventManager.ClickableEvents.ClickUp?.Invoke(_clickData);
            }
        }

        #endregion

        #region Abstract Methods

        protected abstract void OnClickDown(ClickData clickData);

        protected abstract void OnClickHold(ClickData clickData);

        protected abstract void OnClickUp(ClickData clickData);

        #endregion
    }
    
    #region Input Data Struct 

    [Serializable]
    public struct ClickData
    {
        public enum ClickState
        {
            Down,
            Hold,
            Up
        }

        public ClickState clickState;
        
        public enum ClickedObject
        {
            Player,
            Enemy,
            Environment
        }

        public ClickedObject clickedObject;
    }

    #endregion
    
}




