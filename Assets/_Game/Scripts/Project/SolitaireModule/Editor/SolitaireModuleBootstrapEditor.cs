#if UNITY_EDITOR
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    [CustomEditor(typeof(SolitaireModuleBootstrap))]
    public sealed class SolitaireModuleBootstrapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var bootstrap = (SolitaireModuleBootstrap)target;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Feature Registration", EditorStyles.boldLabel);

            if (GUILayout.Button("Validate Self-Registered Scene"))
            {
                if (bootstrap.Validate(out string error))
                    Debug.Log("[SolitaireModuleBootstrap] Validation passed.");
                else
                    Debug.LogError($"[SolitaireModuleBootstrap] Validation failed: {error}");
            }

            EditorGUILayout.HelpBox(
                "Scene objects and ControllerHost register themselves through EventManager.SolitaireEvents. " +
                "Bootstrap only keeps deck config.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Board Test Senaryoları", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.ValidFourCardMerge, "1 · 4 Kart Merge ✔");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.RejectSameColorJunction, "2 · Kırmızı-Kırmızı ✘");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.ValidKingSequenceToEmpty, "3 · King Serisi Boş Sütun ✔");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.RejectInvalidInternalSequence, "4 · Geçersiz Seri ✘");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.PartialSequenceTwoCards, "5 · Kısmi Seri (2 kart) ✘");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.ValidTwoCardMerge, "6 · 2 Kart Merge ✔");
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Endgame / Flow Senaryoları", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.EndGameSuccess, "7 · Endgame Success");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.EndGameFail, "8 · Endgame Fail");
                DrawScenarioButton(bootstrap, SolitaireDebugScenarioId.Restart, "9 · Restart");
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Play Mode'da butona bas veya 'Use Debug Scenario On Start' işaretle.\n" +
                    "Sütunlar: T0=1. kolon ... T6=7. kolon (soldan sağa).",
                    MessageType.Info);
            }
        }

        private static void DrawScenarioButton(
            SolitaireModuleBootstrap bootstrap,
            SolitaireDebugScenarioId scenarioId,
            string label)
        {
            if (!GUILayout.Button(label))
                return;

            bootstrap.ApplyDebugScenarioInPlayMode(scenarioId);
        }
    }
}
#endif
