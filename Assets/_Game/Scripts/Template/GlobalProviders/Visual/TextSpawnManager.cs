using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Helper.Pooling;
using _Game.Scripts.ScriptableObjects.Predefined;
using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Input;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using DG.Tweening;
using Handler.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Template.GlobalProviders.Visual
{
    public class TextSpawnManager : MonoBehaviour, IDamageableAction, IClickableAction
    {
        #region Serialized Fields

        [SerializeField] private GameObject textPrefab;

        private Transform _cameraTransform;

        [SerializeField] private IconProviderSO _iconProviderSo;

        [SerializeField] private CollectableType _collectableType;

        [SerializeField] private float duration;

        [SerializeField] private bool isIconUsed;

        private Sequence _activeSequence;

        private bool isDestroyed;

        #endregion

        #region Constants

        private const float SpawnScale = .8f;

        #endregion


        #region Private Variables

        private GameObject textInstance;

        #endregion
        
        
        #region Unity Methods

        private void Awake()
        {
            Camera mainCamera = Camera.main;
            _cameraTransform = mainCamera != null ? mainCamera.transform : null;
            ValidateReferences();
        }

        #endregion


        #region Public Methods

        [Button]
        public void SpawnText(float value, CollectableType type)
        {
            var position = GetSpawnPosition();
            textInstance = SpawnTextObject(position);
            if (textInstance == null)
            {
                TDebug.LogWarning($"{nameof(TextSpawnManager)} could not spawn text because text prefab is missing.");
                return;
            }

            var text = textInstance.GetComponentInChildren<TextMeshPro>();
            var spriteRenderer = textInstance.GetComponentInChildren<SpriteRenderer>();
            if (text == null || spriteRenderer == null)
            {
                TDebug.LogWarning($"{nameof(TextSpawnManager)} spawned text prefab must contain TextMeshPro and SpriteRenderer.");
                GameObjectPool.Despawn(textInstance);
                return;
            }

            spriteRenderer.enabled = isIconUsed;

            spriteRenderer.DOKill();
            spriteRenderer.sprite = _iconProviderSo != null ? _iconProviderSo.GetCollectableIcon(type) : null;

            SetTextAndScale(text, value);
            ApplyColors(text, spriteRenderer);
            PerformAnimation(textInstance, text, spriteRenderer);
        }

        #endregion

        #region Private Methods

        private Vector3 GetSpawnPosition()
        {
            return transform.position + GetRandomOffset();
        }

        private GameObject SpawnTextObject(Vector3 position)
        {
            return GameObjectPool.Spawn(textPrefab, position, Quaternion.identity);
        }

        private void SetTextAndScale(TMP_Text text, float value)
        {
            text.text = $"{value.ToInt()}";

            var scaleMultiplier = Mathf.Clamp(value/100f, 1f, 1.5f);

            text.transform.localScale = Vector3.one*(SpawnScale*scaleMultiplier);
        }

        private Vector3 GetRandomOffset()
        {
            var randomOffsetX = Random.Range(-0.5f, 0.5f);
            var randomOffsetY = Random.Range(-0.5f, 0.5f);
            return new Vector3(randomOffsetX, randomOffsetY);
        }

        private void ApplyColors(TextMeshPro text, SpriteRenderer sprite)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
        }

        private void PerformAnimation(GameObject textInstance, TextMeshPro text, SpriteRenderer spriteRenderer)
        {
            if (isDestroyed) return;
            if (_cameraTransform != null)
            {
                text.transform.LookAtCamera(_cameraTransform.rotation);
            }

            ResetSpawnedTextTransform(textInstance);
            var sequence = DOTween.Sequence();
            _activeSequence = sequence;

            sequence.Append(textInstance.transform.DOMoveY(textInstance.transform.localPosition.y + 1f, duration));
            sequence.Join(text.DOFade(0, duration));
            sequence.Join(spriteRenderer.DOFade(0, duration));
            sequence.OnComplete(() => GameObjectPool.Despawn(textInstance.gameObject));
        }

        private void ResetSpawnedTextTransform(GameObject spawnedText)
        {
            if (spawnedText.TryGetComponent(out RectTransform rectTransform))
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

            spawnedText.transform.position = transform.position;
        }

        private void ValidateReferences()
        {
            if (textPrefab == null)
            {
                TDebug.LogWarning($"{nameof(TextSpawnManager)} requires {nameof(textPrefab)}.");
            }

            if (_iconProviderSo == null && isIconUsed)
            {
                TDebug.LogWarning($"{nameof(TextSpawnManager)} uses icons but has no IconProviderSO.");
            }
        }

        #endregion

        public void Initialize(DamageableObject damageableObject) { }

        public void TakeDamage(float damage)
        {
            SpawnText(damage, _collectableType);
        }

        public void Death()
        {
            textInstance?.SetActive(false);
        }

        public void OnDestroy()
        {
            textInstance?.SetActive(false);
        }

        public void HealthChanged(float currentHealth)
        {
            if (!(currentHealth <= 0)) return;
            _activeSequence?.Kill();
            _activeSequence = null;
            isDestroyed = true;
        }

        public void OnClickDown()
        {
            SpawnText(100, _collectableType);
            TDebug.Log("Works");
        }

        public void OnClickHold()
        {
        }

        public void OnClickUp()
        {
        }
    }
}
