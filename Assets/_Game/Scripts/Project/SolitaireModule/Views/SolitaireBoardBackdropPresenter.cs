using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SolitaireBoardBackdropPresenter : MonoBehaviour
    {
        private const int BackdropSortingOrder = -1000;
        private const float BackdropLocalZ = 11f;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private Camera _boardCamera;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sortingOrder = BackdropSortingOrder;
            transform.localPosition = new Vector3(0f, 0f, BackdropLocalZ);

            if (transform.parent != null)
                _boardCamera = transform.parent.GetComponent<Camera>();
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.BoardViewportSizeChanged += HandleViewportChanged;
            RefreshLayout();
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.BoardViewportSizeChanged -= HandleViewportChanged;
        }

        private void HandleViewportChanged()
        {
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || _boardCamera == null)
                return;

            float viewportHeight = _boardCamera.orthographicSize * 2f;
            float viewportWidth = viewportHeight * _boardCamera.aspect;
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float coverScale = Mathf.Max(
                viewportWidth / Mathf.Max(spriteSize.x, 0.0001f),
                viewportHeight / Mathf.Max(spriteSize.y, 0.0001f));

            transform.localScale = new Vector3(coverScale, coverScale, 1f);
        }
    }
}
