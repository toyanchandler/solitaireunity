using System;
using System.Collections;
using System.IO;
using _Game.Scripts.Managers.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace _Game.Scripts.UI.Screens
{
    public class StartView : MonoBehaviour
    {
        private const string SplashVideoFileName = "splashscreenvideo.mp4";
        private const float DefaultVideoAspectRatio = 448f / 656f;
        private const float PrepareTimeout = 5f;
        private const float VideoFadeDuration = 0.35f;

        private RectTransform _blackBackdrop;
        private RawImage _videoImage;
        private RectTransform _videoRect;
        private VideoPlayer _videoPlayer;
        private Coroutine _playRoutine;
        private bool _levelStartInvoked;
        private bool _playbackStarted;
        private Vector2 _lastParentSize;

        private void OnEnable()
        {
            _levelStartInvoked = false;
            _playbackStarted = false;

            DisableAuthoredStartContent();
            EnsureSplashHierarchy();

            _playRoutine = StartCoroutine(PlaySplashRoutine());
        }

        private void OnDisable()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= HandleVideoFinished;
                _videoPlayer.errorReceived -= HandleVideoError;
                _videoPlayer.Stop();
            }
        }

        private void Update()
        {
            FitVideoInsideView();

            if (!_levelStartInvoked &&
                _playbackStarted &&
                _videoPlayer != null &&
                _videoPlayer.isPrepared &&
                _videoPlayer.length > 0.01f &&
                _videoPlayer.time >= _videoPlayer.length - 0.05f)
            {
                CompleteSplash();
            }
        }

        private void DisableAuthoredStartContent()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (_blackBackdrop != null && child == _blackBackdrop)
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        private void EnsureSplashHierarchy()
        {
            if (_blackBackdrop == null)
            {
                GameObject backdropObject = new GameObject(
                    "SplashVideoBlackBackdrop",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));

                _blackBackdrop = backdropObject.GetComponent<RectTransform>();
                _blackBackdrop.SetParent(transform, false);
                _blackBackdrop.anchorMin = Vector2.zero;
                _blackBackdrop.anchorMax = Vector2.one;
                _blackBackdrop.anchoredPosition = Vector2.zero;
                _blackBackdrop.sizeDelta = Vector2.zero;

                Image backdropImage = backdropObject.GetComponent<Image>();
                backdropImage.color = Color.black;
                backdropImage.raycastTarget = false;
            }

            _blackBackdrop.gameObject.SetActive(true);
            _blackBackdrop.SetAsLastSibling();

            if (_videoImage == null)
            {
                GameObject videoObject = new GameObject(
                    "SplashVideoImage",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage));

                _videoRect = videoObject.GetComponent<RectTransform>();
                _videoRect.SetParent(_blackBackdrop, false);
                _videoRect.anchorMin = new Vector2(0.5f, 0.5f);
                _videoRect.anchorMax = new Vector2(0.5f, 0.5f);
                _videoRect.pivot = new Vector2(0.5f, 0.5f);
                _videoRect.anchoredPosition = Vector2.zero;

                _videoImage = videoObject.GetComponent<RawImage>();
                _videoImage.raycastTarget = false;
            }

            _videoImage.gameObject.SetActive(true);
            SetVideoAlpha(0f);
            FitVideoInsideView(force: true);

            if (_videoPlayer == null)
            {
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();
                _videoPlayer.playOnAwake = false;
                _videoPlayer.isLooping = false;
                _videoPlayer.waitForFirstFrame = true;
                _videoPlayer.skipOnDrop = true;
                _videoPlayer.renderMode = VideoRenderMode.APIOnly;
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            }
        }

        private IEnumerator PlaySplashRoutine()
        {
            string videoUrl = GetSplashVideoUrl();

            if (string.IsNullOrEmpty(videoUrl))
            {
                Debug.LogWarning($"[{nameof(StartView)}] Splash video is missing.");
                CompleteSplash();
                yield break;
            }

            _videoPlayer.loopPointReached -= HandleVideoFinished;
            _videoPlayer.errorReceived -= HandleVideoError;
            _videoPlayer.loopPointReached += HandleVideoFinished;
            _videoPlayer.errorReceived += HandleVideoError;
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = videoUrl;
            _videoPlayer.controlledAudioTrackCount = 1;
            _videoPlayer.EnableAudioTrack(0, true);
            _videoPlayer.SetDirectAudioMute(0, true);
            _videoPlayer.Prepare();

            float elapsed = 0f;
            while (isActiveAndEnabled && !_videoPlayer.isPrepared && elapsed < PrepareTimeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!isActiveAndEnabled)
                yield break;

            if (!_videoPlayer.isPrepared)
            {
                Debug.LogWarning($"[{nameof(StartView)}] Splash video prepare timed out: {videoUrl}");
                CompleteSplash();
                yield break;
            }

            _videoImage.texture = _videoPlayer.texture;
            FitVideoInsideView(force: true);
            _videoPlayer.Play();
            _playbackStarted = true;
            yield return FadeVideoInRoutine();
        }

        private string GetSplashVideoUrl()
        {
            string path = Path.Combine(Application.streamingAssetsPath, SplashVideoFileName);

            if (path.Contains("://"))
                return path;

            if (!File.Exists(path))
                return string.Empty;

            return new Uri(path).AbsoluteUri;
        }

        private void FitVideoInsideView(bool force = false)
        {
            if (_blackBackdrop == null || _videoRect == null)
                return;

            Rect parentRect = _blackBackdrop.rect;
            Vector2 parentSize = parentRect.size;

            if (!force && parentSize == _lastParentSize)
                return;

            _lastParentSize = parentSize;

            if (parentSize.x <= 0f || parentSize.y <= 0f)
                return;

            float aspectRatio = DefaultVideoAspectRatio;

            if (_videoPlayer != null && _videoPlayer.width > 0 && _videoPlayer.height > 0)
                aspectRatio = (float)_videoPlayer.width / _videoPlayer.height;

            float targetHeight = parentSize.y;
            float targetWidth = targetHeight * aspectRatio;

            if (targetWidth > parentSize.x)
            {
                targetWidth = parentSize.x;
                targetHeight = targetWidth / aspectRatio;
            }

            _videoRect.sizeDelta = new Vector2(targetWidth, targetHeight);
            _videoRect.anchoredPosition = Vector2.zero;
        }

        private IEnumerator FadeVideoInRoutine()
        {
            float elapsed = 0f;
            SetVideoAlpha(0f);

            while (isActiveAndEnabled && elapsed < VideoFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetVideoAlpha(Mathf.Clamp01(elapsed / VideoFadeDuration));
                yield return null;
            }

            SetVideoAlpha(1f);
        }

        private void SetVideoAlpha(float alpha)
        {
            if (_videoImage == null)
                return;

            Color color = _videoImage.color;
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = alpha;
            _videoImage.color = color;
        }

        private void HandleVideoFinished(VideoPlayer source)
        {
            CompleteSplash();
        }

        private void HandleVideoError(VideoPlayer source, string message)
        {
            Debug.LogWarning($"[{nameof(StartView)}] Splash video error: {message}", this);
            CompleteSplash();
        }

        private void CompleteSplash()
        {
            if (_levelStartInvoked)
                return;

            _levelStartInvoked = true;
            EventManager.InGameEvents.LevelStart?.Invoke();
        }
    }
}
