using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class SolitairePulseRingView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ringRenderer;
        [SerializeField] private float startScale = 0.35f;
        [SerializeField] private float endScale = 1.15f;
        [SerializeField] private float duration = 0.34f;
        [SerializeField] private Color ringColor = new Color(0.35f, 0.72f, 1f, 0.85f);

        private Coroutine _playRoutine;

        public void Play()
        {
            if (ringRenderer == null)
                return;

            if (_playRoutine != null)
                StopCoroutine(_playRoutine);

            _playRoutine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            ringRenderer.enabled = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                float scale = Mathf.Lerp(startScale, endScale, eased);
                transform.localScale = new Vector3(scale, scale, 1f);

                Color color = ringColor;
                color.a = ringColor.a * (1f - t);
                ringRenderer.color = color;

                yield return null;
            }

            ringRenderer.enabled = false;
            transform.localScale = Vector3.one;
            _playRoutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (ringRenderer == null)
                ringRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
#endif
    }
}
