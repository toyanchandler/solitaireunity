#if UNITY_EDITOR
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    [CustomEditor(typeof(SolitaireDebugScenarioRunner))]
    public sealed class SolitaireDebugScenarioRunnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var runner = (SolitaireDebugScenarioRunner)target;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Quick Apply (Play Mode)", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                DrawScenarioButton(runner, SolitaireDebugScenarioId.ValidFourCardMerge, "1 · 4 Kart Merge ✔");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.RejectSameColorJunction, "2 · Kırmızı-Kırmızı ✘");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.ValidKingSequenceToEmpty, "3 · King Serisi Boş Sütun ✔");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.RejectInvalidInternalSequence, "4 · Geçersiz Seri ✘");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.PartialSequenceTwoCards, "5 · Kısmi Seri (2 kart) ✘");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.ValidTwoCardMerge, "6 · 2 Kart Merge ✔");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.EndGameSuccess, "7 · Endgame Success");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.EndGameFail, "8 · Endgame Fail");
                DrawScenarioButton(runner, SolitaireDebugScenarioId.Restart, "9 · Restart");
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Play Mode'a gir, sonra senaryo butonuna bas veya Replace Level Start Deal ile otomatik yükle.", MessageType.Info);
        }

        private static void DrawScenarioButton(
            SolitaireDebugScenarioRunner runner,
            SolitaireDebugScenarioId scenarioId,
            string label)
        {
            if (!GUILayout.Button(label))
                return;

            runner.ApplyScenario(scenarioId);
        }
    }
}
#endif
