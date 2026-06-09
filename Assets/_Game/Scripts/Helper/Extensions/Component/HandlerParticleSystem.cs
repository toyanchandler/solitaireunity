using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerParticleSystem
    {
        public static void PlayParticles(this List<ParticleSystem> sysList)
        {
            int count = sysList.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                ParticleSystem sys = sysList[i];
                if (sys != null && !sys.isPlaying)
                {
                    sys.Play();
                }
            }
        }
        
        public static void StopParticles(this ParticleSystem[] sysList)
        {
            for (var i = sysList.Length - 1; i >= 0; i--)
            {
                ParticleSystem sys = sysList[i];
                if (sys != null && sys.isPlaying)
                {
                    sys.Stop();
                }
            }
        }
        
        public static void PlayParticlesSerially(this List<ParticleSystem> particles, MonoBehaviour handle, float interval, System.Action OnComplete = null)
        {
            handle.StartCoroutine(AsyncInterval());
            IEnumerator AsyncInterval()
            {
                var waiter = new WaitForSeconds(interval);
                if (particles != null && particles.Count > 0)
                {
                    for (var i = 0; i < particles.Count; i++)
                    {
                        var eff = particles[i];
                        if (eff == null) { continue; }
                        eff.Play();
                        yield return waiter;
                    }
                }
                OnComplete?.Invoke();
            }
        }
        
        public static void StopParticlesSerially(this List<ParticleSystem> particles, MonoBehaviour handle, float interval, System.Action OnComplete = null)
        {
            handle.StartCoroutine(AsyncInterval());
            IEnumerator AsyncInterval()
            {
                var waiter = new WaitForSeconds(interval);
                if (particles != null && particles.Count > 0)
                {
                    for (var i = 0; i < particles.Count; i++)
                    {
                        var eff = particles[i];
                        if (eff == null) { continue; }
                        eff.Stop();
                        yield return waiter;
                    }
                }
                OnComplete?.Invoke();
            }
        }
        
        public static void PlayDelayedParticlesWithCoroutine(this List<ParticleSystem> particles, MonoBehaviour handle, float delay, System.Action OnComplete = null)
        {
            handle.StartCoroutine(AsyncInterval());
            IEnumerator AsyncInterval()
            {
                yield return new WaitForSeconds(delay);
                if (particles != null && particles.Count > 0)
                {
                    for (var i = 0; i < particles.Count; i++)
                    {
                        var eff = particles[i];
                        if (eff == null) { continue; }
                        eff.Play();
                    }
                }
                OnComplete?.Invoke();
            }
        }
        
        public static void StopDelayedParticlesWithCoroutine(this List<ParticleSystem> particles, MonoBehaviour handle, float delay, System.Action OnComplete = null)
        {
            handle.StartCoroutine(AsyncInterval());
            IEnumerator AsyncInterval()
            {
                yield return new WaitForSeconds(delay);
                if (particles != null && particles.Count > 0)
                {
                    for (var i = 0; i < particles.Count; i++)
                    {
                        var eff = particles[i];
                        if (eff == null) { continue; }
                        eff.Stop();
                    }
                }
                OnComplete?.Invoke();
            }
        }
    }
}