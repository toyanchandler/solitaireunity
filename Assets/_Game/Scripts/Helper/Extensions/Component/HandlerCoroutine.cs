using System;
using System.Collections;
using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerCoroutine
    {
        public static void StopCoroutineSafe(this MonoBehaviour self, ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                self.StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        
        public static void SafeCoroutine(this MonoBehaviour self, IEnumerator coroutine, ref Coroutine handle)
        {
            self.StopCoroutineSafe(ref handle);
            handle = self.StartCoroutine(coroutine);
        }
        
        public static Coroutine RunUntilWithInterval(this MonoBehaviour mono, Func<bool> condition, Action callback, float interval)
        {
            return mono.StartCoroutine(RunUntilConditionMetCoroutineWithInterval(condition, callback, interval));
        }
        
        public static Coroutine RepeatAction(this MonoBehaviour mono, Action action, int repetitions, float interval)
        {
            return mono.StartCoroutine(RepeatAction(action, repetitions, interval));
        }
        
        #region Providers

        public static IEnumerator RepeatAction(Action action, int repetitions, float interval)
        {
            for (int i = 0; i < repetitions; i++)
            {
                action.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }
        
        private static IEnumerator RunUntilConditionMetCoroutineWithInterval(Func<bool> predicate, Action action, float interval)
        {
            while (!predicate())
            {
                yield return new WaitForSeconds(interval);
            }
            action.Invoke();
        }

        #endregion
    }
}