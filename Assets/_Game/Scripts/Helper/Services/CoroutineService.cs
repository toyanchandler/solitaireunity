using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Helper.Services
{
    public class CoroutineService
    {
        private readonly MonoBehaviour _monoBehaviour;
        private readonly List<Coroutine> _activeCoroutines = new List<Coroutine>();

        public CoroutineService(MonoBehaviour monoBehaviour)
        {
            _monoBehaviour = monoBehaviour;
        }

        public Coroutine StartUpdateRoutine(Action methodToRun,Func<bool> condition)
        {
            Coroutine coroutine = _monoBehaviour.StartCoroutine(UpdateRoutine(methodToRun, condition));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }
        
        public Coroutine StartLateUpdateRoutine(Action methodToRun, Func<bool> condition)
        {
            Coroutine coroutine = _monoBehaviour.StartCoroutine(LateUpdateRoutine(methodToRun, condition));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }
        
        public Coroutine StartFixedUpdateRoutine(Action methodToRun, Func<bool> condition)
        {
            Coroutine coroutine = _monoBehaviour.StartCoroutine(FixedUpdateRoutine(methodToRun, condition));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }

        public Coroutine StartDelayedRoutine(Action methodToRun, float delay)
        {
            Coroutine coroutine = _monoBehaviour.StartCoroutine(DelayedRoutine(methodToRun, delay));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }
        
        public Coroutine StartIntervalRoutine(Action methodToRun, float interval, Func<bool> condition)
        {
            Coroutine coroutine = _monoBehaviour.StartCoroutine(IntervalRoutine(methodToRun, interval, condition));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }

        private IEnumerator IntervalRoutine(Action methodToRun, float interval, Func<bool> condition)
        {
            while (condition())
            {
                methodToRun();
                yield return new WaitForSeconds(interval);
            }
        }

        public void StopAll()
        {
            foreach (var coroutine in _activeCoroutines)
            {
                _monoBehaviour.StopCoroutine(coroutine);
            }
            _activeCoroutines.Clear();
        }
    
        public void Stop(Coroutine coroutine)
        {
            if (coroutine != null && _activeCoroutines.Contains(coroutine))
            {
                _monoBehaviour.StopCoroutine(coroutine);
                _activeCoroutines.Remove(coroutine);
            }
        }

        private IEnumerator UpdateRoutine(Action method, Func<bool> condition)
        {
            while (condition())
            {
                method();
                yield return null;
            }
        }
        
        private IEnumerator FixedUpdateRoutine(Action method, Func<bool> condition)
        {
            while (condition())
            {
                method();
                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator DelayedRoutine(Action method, float delay)
        {
            yield return new WaitForSeconds(delay);
            method();
        }
        
        private IEnumerator LateUpdateRoutine(Action method, Func<bool> condition)
        {
            while (condition())
            {
                yield return new WaitForEndOfFrame();
                method();
            }
        }
    }
}
