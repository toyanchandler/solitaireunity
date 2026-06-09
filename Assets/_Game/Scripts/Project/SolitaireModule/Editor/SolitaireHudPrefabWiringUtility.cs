using _Game.Scripts.Project.SolitaireModule.Views;
using _Game.Scripts.UI.Buttons;
using _Game.Scripts.UI.Screens;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireHudPrefabWiringUtility
    {
        private const string InGameScreenPath = "Assets/_Game/Prefabs/Screens/InGameScreen.prefab";

        [MenuItem("Tools/Solitaire/Wire HUD Controls")]
        public static void WireHudControlsMenu()
        {
            WireHudControls();
        }

        public static void WireHudControls()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(InGameScreenPath);

            try
            {
                Transform parent = root.transform;
                Transform undo = parent.Find("UndoButton");

                if (undo == null)
                    throw new MissingReferenceException($"{InGameScreenPath} is missing UndoButton.");

                Transform hint = EnsureButton<SolitaireHintButton>(parent, undo, "HintButton", "HINT", 28f);
                Transform autoComplete = EnsureButton<SolitaireAutoCompleteButton>(parent, undo, "AutoCompleteButton", "AUTO", 23f);

                if (root.GetComponent<SolitaireHintPresenter>() == null)
                    root.AddComponent<SolitaireHintPresenter>();

                SolitaireHudLayoutAnimator layout = root.GetComponent<SolitaireHudLayoutAnimator>();

                if (layout == null)
                    layout = root.AddComponent<SolitaireHudLayoutAnimator>();

                var serialized = new SerializedObject(layout);
                Assign(serialized, "movesCounter", parent.Find("MovesCounter")?.GetComponent<RectTransform>());
                Assign(serialized, "scoreCounter", parent.Find("ScoreCounter")?.GetComponent<RectTransform>());
                Assign(serialized, "undoButton", undo.GetComponent<RectTransform>());
                Assign(serialized, "hintButton", hint.GetComponent<RectTransform>());
                Assign(serialized, "autoCompleteButton", autoComplete.GetComponent<RectTransform>());
                Assign(serialized, "undoButtonComponent", undo.GetComponent<Button>());
                Assign(serialized, "hintButtonComponent", hint.GetComponent<Button>());
                Assign(serialized, "autoCompleteButtonComponent", autoComplete.GetComponent<Button>());
                Assign(serialized, "portraitCounterSize", new Vector2(146f, 46f));
                Assign(serialized, "portraitButtonSize", new Vector2(106f, 46f));
                Assign(serialized, "portraitCounterY", 0.925f);
                Assign(serialized, "portraitButtonY", 0.065f);
                Assign(serialized, "landscapeCounterSize", new Vector2(132f, 42f));
                Assign(serialized, "landscapeButtonSize", new Vector2(106f, 42f));
                Assign(serialized, "landscapeY", 0.92f);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, InGameScreenPath);
                Debug.Log($"[SolitaireHudPrefabWiringUtility] Wired HUD controls in {InGameScreenPath}.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform EnsureButton<TButton>(Transform parent, Transform source, string name, string label, float fontSize)
            where TButton : ButtonBase
        {
            Transform button = parent.Find(name);

            if (button == null)
            {
                button = Object.Instantiate(source, parent);
                button.name = name;
            }

            RemoveOtherSolitaireButtons<TButton>(button.gameObject);

            if (button.GetComponent<TButton>() == null)
                button.gameObject.AddComponent<TButton>();

            Transform labelTransform = button.Find("Label");

            if (labelTransform != null && labelTransform.TryGetComponent(out TextMeshProUGUI tmp))
            {
                tmp.text = label;
                tmp.fontSize = fontSize;
            }

            return button;
        }

        private static void RemoveOtherSolitaireButtons<TKeep>(GameObject target)
            where TKeep : ButtonBase
        {
            ButtonBase[] buttons = target.GetComponents<ButtonBase>();

            for (int i = 0; i < buttons.Length; i++)
            {
                ButtonBase button = buttons[i];

                if (button is TKeep)
                    continue;

                Object.DestroyImmediate(button, true);
            }
        }

        private static void Assign(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);

            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void Assign(SerializedObject serialized, string propertyName, Vector2 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);

            if (property != null)
                property.vector2Value = value;
        }

        private static void Assign(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);

            if (property != null)
                property.floatValue = value;
        }
    }
}
