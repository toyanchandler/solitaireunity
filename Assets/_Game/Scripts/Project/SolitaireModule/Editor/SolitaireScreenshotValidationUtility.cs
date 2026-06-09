using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Managers.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireScreenshotValidationUtility
    {
        private const string ScenePath = "Assets/_Game/Scenes/_TemplateScene.unity";
        private const string ScreenshotFolder = "Assets/Screenshots";
        private const int MaxFrames = 180;
        private const int LevelStartFrame = 45;
        private const int CaptureFrame = 110;

        private static readonly string[] HudElementNames =
        {
            "MovesCounter",
            "ScoreCounter",
            "UndoButton",
            "HintButton",
            "AutoCompleteButton"
        };

        private static string _screenshotPath;
        private static int _frame;
        private static bool _captureStarted;

        public static void CapturePortraitAndExit()
        {
            StartCapture(390, 844, "solitaire-ui-portrait-hint-autocomplete-20260609.png");
        }

        public static void CaptureLandscapeAndExit()
        {
            StartCapture(844, 390, "solitaire-ui-landscape-hint-autocomplete-20260609.png");
        }

        private static void StartCapture(int width, int height, string fileName)
        {
            Directory.CreateDirectory(ScreenshotFolder);
            _screenshotPath = Path.GetFullPath(Path.Combine(ScreenshotFolder, fileName));
            _frame = 0;
            _captureStarted = false;

            if (File.Exists(_screenshotPath))
                File.Delete(_screenshotPath);

            EditorSceneManager.OpenScene(ScenePath);
            Screen.SetResolution(width, height, false);
            Debug.Log($"[SolitaireScreenshotValidationUtility] Starting capture {width}x{height}: {_screenshotPath}");

            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorApplication.EnterPlaymode();
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            _frame++;

            if (_frame == LevelStartFrame)
            {
                EventManager.InGameEvents.LevelStart?.Invoke();
                Debug.Log("[SolitaireScreenshotValidationUtility] Invoked LevelStart for gameplay UI capture.");
            }

            if (_frame == CaptureFrame)
            {
                ValidateHudElements();
                StartEndOfFrameCapture();
                return;
            }

            if (_frame > MaxFrames)
                Finish(1, $"Capture timed out: {_screenshotPath}");
        }

        private static void StartEndOfFrameCapture()
        {
            if (_captureStarted)
                return;

            _captureStarted = true;
            var runnerObject = new GameObject("SolitaireScreenshotValidationRunner");
            UnityEngine.Object.DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<CaptureRunner>().Begin(_screenshotPath);
        }

        private static void ValidateHudElements()
        {
            var rects = new List<NamedRect>(HudElementNames.Length);

            for (int i = 0; i < HudElementNames.Length; i++)
            {
                string elementName = HudElementNames[i];
                GameObject element = GameObject.Find(elementName);

                if (element == null)
                {
                    Debug.LogError($"[SolitaireScreenshotValidationUtility] Missing HUD element: {elementName}");
                    continue;
                }

                if (!element.activeInHierarchy)
                    Debug.LogError($"[SolitaireScreenshotValidationUtility] Inactive HUD element: {elementName}");

                if (!element.TryGetComponent(out RectTransform rectTransform))
                {
                    Debug.LogError($"[SolitaireScreenshotValidationUtility] HUD element has no RectTransform: {elementName}");
                    continue;
                }

                Rect screenRect = GetScreenRect(rectTransform);
                rects.Add(new NamedRect(elementName, screenRect));
                Debug.Log($"[SolitaireScreenshotValidationUtility] {elementName} rect={screenRect}");
            }

            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = i + 1; j < rects.Count; j++)
                {
                    if (rects[i].Rect.Overlaps(rects[j].Rect))
                        Debug.LogError($"[SolitaireScreenshotValidationUtility] HUD overlap: {rects[i].Name} / {rects[j].Name}");
                }
            }
        }

        private static Rect GetScreenRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static void Finish(int exitCode, string message)
        {
            EditorApplication.update -= Tick;

            if (exitCode == 0)
                Debug.Log($"[SolitaireScreenshotValidationUtility] {message}");
            else
                Debug.LogError($"[SolitaireScreenshotValidationUtility] {message}");

            EditorApplication.ExitPlaymode();
            EditorApplication.Exit(exitCode);
        }

        private readonly struct NamedRect
        {
            public readonly string Name;
            public readonly Rect Rect;

            public NamedRect(string name, Rect rect)
            {
                Name = name;
                Rect = rect;
            }
        }

        private sealed class CaptureRunner : MonoBehaviour
        {
            private string _path;

            public void Begin(string path)
            {
                _path = path;
                StartCoroutine(CaptureAtEndOfFrame());
            }

            private IEnumerator CaptureAtEndOfFrame()
            {
                yield return new WaitForEndOfFrame();

                Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

                if (screenshot == null)
                {
                    Finish(1, $"Capture returned null texture: {_path}");
                    yield break;
                }

                File.WriteAllBytes(_path, screenshot.EncodeToPNG());
                UnityEngine.Object.Destroy(screenshot);
                Finish(0, $"Capture completed: {_path}");
            }
        }
    }
}
