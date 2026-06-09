using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Audio
{
    public static class CrossFadeAudioService
    {
        #region Public Methods

        public static IEnumerator CrossFadeCoroutine(AudioClip newAudioClip, float fadeDuration, AudioSource audioSource)
        {
            if (audioSource == null || newAudioClip == null)
            {
                yield break;
            }

            if (fadeDuration <= 0f)
            {
                audioSource.clip = newAudioClip;
                audioSource.volume = 1f;
                audioSource.Play();
                yield break;
            }

            yield return FadeAudioCoroutine(audioSource, 0f, fadeDuration);
        
            audioSource.clip = newAudioClip;
            audioSource.Play();

            yield return FadeAudioCoroutine(audioSource, 1f, fadeDuration);
        }

        #endregion

        #region Private Methods

        private static IEnumerator FadeAudioCoroutine(AudioSource audioSource, float targetVolume, float duration)
        {
            if (audioSource == null)
            {
                yield break;
            }

            float startVolume = audioSource.volume;
            float startTime = Time.time;

            while (Mathf.Abs(audioSource.volume - targetVolume) > 0.01f)
            {
                float elapsedTime = Time.time - startTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            audioSource.volume = targetVolume; // Ensuring the target volume is set exactly to avoid any floating point imprecision.
        }

        #endregion
    }
}
