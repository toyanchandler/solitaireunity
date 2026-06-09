#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    [InitializeOnLoad]
    public static class SolitaireDebugScenarioPanelAutoBake
    {
        private const string SessionKey = "SolitaireDebugScenarioCanvasBaked_v1";

        static SolitaireDebugScenarioPanelAutoBake()
        {
            EditorApplication.delayCall += TryBakeOnce;
        }

        private static void TryBakeOnce()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!File.Exists(SolitaireDebugScenarioPanelBuilder.SceneAssetPath))
                return;

            if (File.ReadAllText(SolitaireDebugScenarioPanelBuilder.SceneAssetPath)
                .Contains("SolitaireDebugScenarioCanvas"))
            {
                SessionState.SetBool(SessionKey, true);
                return;
            }

            try
            {
                SolitaireDebugScenarioPanelBuilder.BakeDebugScenarioCanvas(saveScene: true);
                SessionState.SetBool(SessionKey, true);
                Debug.Log("Solitaire debug scenario canvas auto-baked into _TemplateScene.");
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Solitaire debug canvas auto-bake skipped: {exception.Message}");
            }
        }
    }

    public static class SolitaireDebugScenarioPanelBuilder
    {
        public const string SceneAssetPath = "Assets/_Game/Scenes/_TemplateScene.unity";
        private const string PrefabPath = "Assets/_Game/Prefabs/_InGame/Solitaire/SolitaireDebugScenarioCanvas.prefab";
        private const string TmpFontGuid = "c1482dbce654844b9ba1c753cbea5d80";

        [MenuItem("Tools/Solitaire/Bake Debug Scenario Canvas")]
        public static void BakeDebugScenarioCanvasMenu()
        {
            BakeDebugScenarioCanvas(saveScene: true);
            Debug.Log("Solitaire debug scenario canvas baked into main scene.");
        }

        public static void BakeDebugScenarioCanvas(bool saveScene)
        {
            var scene = EditorSceneManager.OpenScene(SceneAssetPath, OpenSceneMode.Single);
            SolitaireModuleBootstrap bootstrap = Object.FindFirstObjectByType<SolitaireModuleBootstrap>();

            if (bootstrap == null)
                throw new System.InvalidOperationException("SolitaireModuleBootstrap was not found in the main scene.");

            Transform existing = GameObject.Find("SolitaireDebugScenarioCanvas")?.transform;
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            GameObject canvasRoot = BuildCanvasHierarchy(bootstrap);
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasRoot, PrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(scene);

            if (saveScene)
                EditorSceneManager.SaveScene(scene);
        }

        private static GameObject BuildCanvasHierarchy(SolitaireModuleBootstrap bootstrap)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(TmpFontGuid));

            GameObject canvasRoot = new GameObject(
                "SolitaireDebugScenarioCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(SolitaireDebugScenarioPanel));

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystemExists();

            SolitaireDebugScenarioPanel panel = canvasRoot.GetComponent<SolitaireDebugScenarioPanel>();

            Button debugButton = CreateDebugToggleButton(
                canvasRoot.transform,
                "DebugToggleButton",
                "DEBUG",
                font,
                new Color(0.12f, 0.16f, 0.22f, 0.92f));

            GameObject panelRoot = CreatePanelRoot(canvasRoot.transform);
            Image panelBackground = panelRoot.GetComponent<Image>();
            panelBackground.color = new Color(0.05f, 0.08f, 0.12f, 0.94f);

            TextMeshProUGUI titleLabel = CreateAnchoredText(
                panelRoot.transform,
                "TitleLabel",
                "Solitaire Debug Senaryoları",
                font,
                34,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(24f, -108f),
                new Vector2(-24f, -36f));

            TextMeshProUGUI selectedLabel = CreateAnchoredText(
                panelRoot.transform,
                "SelectedScenarioLabel",
                "Seçili senaryo yok",
                font,
                22,
                FontStyles.Normal,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(24f, -300f),
                new Vector2(-24f, -120f));

            var scenarioButtons = new List<Button>(SolitaireDebugScenarioApplier.OrderedScenarios.Length);
            float startY = -330f;
            const float buttonHeight = 72f;
            const float buttonSpacing = 12f;

            for (int i = 0; i < SolitaireDebugScenarioApplier.OrderedScenarios.Length; i++)
            {
                float y = startY - (i * (buttonHeight + buttonSpacing));
                SolitaireDebugScenarioId scenarioId = SolitaireDebugScenarioApplier.OrderedScenarios[i];
                Button scenarioButton = CreateButton(
                    panelRoot.transform,
                    $"ScenarioButton_{i + 1:00}",
                    SolitaireDebugScenarioApplier.GetButtonLabel(scenarioId),
                    font,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, y),
                    new Vector2(520f, buttonHeight),
                    new Color(0.18f, 0.24f, 0.32f, 1f));
                scenarioButtons.Add(scenarioButton);
            }

            panelRoot.SetActive(false);

            SerializedObject panelSerializedObject = new SerializedObject(panel);
            panelSerializedObject.FindProperty("moduleBootstrap").objectReferenceValue = bootstrap;
            panelSerializedObject.FindProperty("debugToggleButton").objectReferenceValue = debugButton;
            panelSerializedObject.FindProperty("scenarioPanelRoot").objectReferenceValue = panelRoot;
            panelSerializedObject.FindProperty("selectedScenarioLabel").objectReferenceValue = selectedLabel;
            panelSerializedObject.FindProperty("panelTitleLabel").objectReferenceValue = titleLabel;

            SerializedProperty scenarioButtonsProperty = panelSerializedObject.FindProperty("scenarioButtons");
            scenarioButtonsProperty.arraySize = scenarioButtons.Count;

            for (int i = 0; i < scenarioButtons.Count; i++)
                scenarioButtonsProperty.GetArrayElementAtIndex(i).objectReferenceValue = scenarioButtons[i];

            panelSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            return canvasRoot;
        }

        private static GameObject CreatePanelRoot(Transform parent)
        {
            GameObject panelRoot = new GameObject(
                "ScenarioPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(620f, 1180f);

            Image image = panelRoot.GetComponent<Image>();
            image.raycastTarget = true;
            return panelRoot;
        }

        private static Button CreateDebugToggleButton(
            Transform parent,
            string objectName,
            string label,
            TMP_FontAsset font,
            Color backgroundColor)
        {
            Button button = CreateButton(
                parent,
                objectName,
                label,
                font,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(48f, -48f),
                new Vector2(160f, 64f),
                backgroundColor);

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 1f);
            return button;
        }

        private static Button CreateButton(
            Transform parent,
            string objectName,
            string label,
            TMP_FontAsset font,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color backgroundColor)
        {
            GameObject buttonObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            CreateText(
                buttonObject.transform,
                "Label",
                label,
                font,
                28,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            return button;
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string objectName,
            string text,
            TMP_FontAsset font,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.offsetMin = anchorMin == Vector2.zero && anchorMax == Vector2.one ? Vector2.zero : rect.offsetMin;
            rect.offsetMax = anchorMin == Vector2.zero && anchorMax == Vector2.one ? Vector2.zero : rect.offsetMax;

            TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static TextMeshProUGUI CreateAnchoredText(
            Transform parent,
            string objectName,
            string text,
            TMP_FontAsset font,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void EnsureEventSystemExists()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }
    }
}
#endif
