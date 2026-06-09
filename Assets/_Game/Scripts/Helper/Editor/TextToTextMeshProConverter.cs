using _Game.Scripts.Helper.Extensions.System;
using Handler.Extensions;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class TextToTextMeshProConverter : EditorWindow
{
    GameObject targetObject;

    [MenuItem("Tools/Text To TextMeshPro Converter")]
    static void Init()
    {
        TextToTextMeshProConverter window = (TextToTextMeshProConverter)EditorWindow.GetWindow(typeof(TextToTextMeshProConverter));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Convert Text to TextMeshPro in Hierarchy", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Convert"))
        {
            ConvertTextToTextMeshProRecursive(targetObject);
        }
    }

    void ConvertTextToTextMeshProRecursive(GameObject obj)
    {
        if (obj == null)
        {
            TDebug.LogError("Target GameObject cannot be null.");
            return;
        }

        // Convert for the current object
        ConvertTextToTextMeshPro(obj);

        // Recursively convert for all children
        foreach (Transform child in obj.transform)
        {
            ConvertTextToTextMeshProRecursive(child.gameObject);
        }
    }

    void ConvertTextToTextMeshPro(GameObject obj)
    {
        Text oldTextComponent = obj.GetComponent<Text>();

        if (oldTextComponent != null)
        {
            // Copy the necessary values from the old Text component
            string oldText = oldTextComponent.text;
            Color oldColor = oldTextComponent.color;
            Font oldFont = oldTextComponent.font;
            int oldFontSize = oldTextComponent.fontSize;
            bool oldEnableRichText = oldTextComponent.supportRichText;

            // Remove the old Text component
            DestroyImmediate(oldTextComponent);

            // Add TextMeshPro component and set its properties
            TextMeshProUGUI tmpComponent = obj.AddComponent<TextMeshProUGUI>();

            tmpComponent.text = oldText;
            tmpComponent.color = oldColor;
            tmpComponent.fontSize = oldFontSize;
            tmpComponent.enableWordWrapping = oldTextComponent.horizontalOverflow == HorizontalWrapMode.Wrap;
            tmpComponent.richText = oldEnableRichText;

            if (oldFont != null)
            {
                // Assuming TMP fonts are stored in a Resources folder and named the same as the Unity fonts
                TMP_FontAsset tmpFont = Resources.Load<TMP_FontAsset>(oldFont.name);
                if (tmpFont != null)
                {
                    tmpComponent.font = tmpFont;
                }
                else
                {
                    TDebug.LogWarning("Could not find a matching TextMeshPro font for " + oldFont.name);
                }
            }
        }
    }
}
