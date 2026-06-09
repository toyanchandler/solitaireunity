using System;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    /// <summary>
    /// Single authority for the Solitaire board camera. Owns viewport monitoring and screen-to-world conversion.
    /// Other systems subscribe to EventManager.SolitaireEvents instead of holding camera references.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class SolitaireBoardCameraController : MonoBehaviour
    {
        private const float DefaultPortraitOrthographicSize = 5.5f;
        private const float LandscapeTargetHalfWidth = 4.45f;
        private const float LandscapeMinOrthographicSize = 2.45f;
        private const float LandscapeMaxOrthographicSize = 3.45f;

        private Camera _camera;
        private float _portraitOrthographicSize = DefaultPortraitOrthographicSize;
        private Vector2Int _lastPixelSize = Vector2Int.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private float _lastComputedAspect = -1f;
        private float _lastOrthographicSize = -1f;
        private Rect _lastSafeArea;
        private Rect _lastPixelRect;

        public Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<Camera>();

                return _camera;
            }
        }

        private void Awake()
        {
            if (Camera == null)
                throw new InvalidOperationException($"{name} requires a {nameof(UnityEngine.Camera)} component.");

            if (Camera.orthographic && Camera.orthographicSize > 0.01f)
                _portraitOrthographicSize = Camera.orthographicSize;
        }

        private void OnEnable()
        {
            ResetCache();
            ApplyResponsiveOrthographicSize();
            TryNotifyIfChanged(force: true);
            SolitaireFeatureRegistration.RegisterBoardCamera(this);
        }

        private void OnDisable()
        {
            SolitaireFeatureRegistration.UnregisterBoardCamera(this);
        }

        private void LateUpdate()
        {
            ApplyResponsiveOrthographicSize();
            TryNotifyIfChanged(force: false);
        }

        public bool TryScreenToWorld(Vector2 screenPosition, out Vector3 worldPosition)
        {
            worldPosition = default;

            Camera camera = Camera;

            if (camera == null)
                return false;

            Vector3 screen = screenPosition;
            screen.z = Mathf.Abs(camera.transform.position.z);
            worldPosition = camera.ScreenToWorldPoint(screen);
            worldPosition.z = 0f;
            return true;
        }

        private void TryNotifyIfChanged(bool force)
        {
            Camera camera = Camera;

            if (camera == null)
                return;

            Vector2Int pixelSize = new Vector2Int(camera.pixelWidth, camera.pixelHeight);
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            float computedAspect = (float)pixelSize.x / Mathf.Max(1, pixelSize.y);
            float orthographicSize = camera.orthographicSize;
            Rect safeArea = Screen.safeArea;
            Rect pixelRect = camera.pixelRect;

            if (!force
                && pixelSize == _lastPixelSize
                && screenSize == _lastScreenSize
                && Mathf.Approximately(computedAspect, _lastComputedAspect)
                && Mathf.Approximately(orthographicSize, _lastOrthographicSize)
                && safeArea == _lastSafeArea
                && pixelRect == _lastPixelRect)
            {
                return;
            }

            _lastPixelSize = pixelSize;
            _lastScreenSize = screenSize;
            _lastComputedAspect = computedAspect;
            _lastOrthographicSize = orthographicSize;
            _lastSafeArea = safeArea;
            _lastPixelRect = pixelRect;
            SolitaireFeatureRegistration.NotifyBoardViewportSizeChanged();
        }

        private void ApplyResponsiveOrthographicSize()
        {
            Camera camera = Camera;

            if (camera == null || !camera.orthographic)
                return;

            int pixelWidth = Mathf.Max(1, camera.pixelWidth);
            int pixelHeight = Mathf.Max(1, camera.pixelHeight);
            float targetSize;

            if (pixelWidth <= pixelHeight)
            {
                targetSize = _portraitOrthographicSize;
            }
            else
            {
                float aspect = (float)pixelWidth / pixelHeight;
                targetSize = Mathf.Clamp(
                    LandscapeTargetHalfWidth / aspect,
                    LandscapeMinOrthographicSize,
                    LandscapeMaxOrthographicSize);
            }

            if (!Mathf.Approximately(camera.orthographicSize, targetSize))
                camera.orthographicSize = targetSize;
        }

        private void ResetCache()
        {
            _lastPixelSize = Vector2Int.zero;
            _lastScreenSize = Vector2Int.zero;
            _lastComputedAspect = -1f;
            _lastOrthographicSize = -1f;
            _lastSafeArea = default;
            _lastPixelRect = default;
        }
    }
}
