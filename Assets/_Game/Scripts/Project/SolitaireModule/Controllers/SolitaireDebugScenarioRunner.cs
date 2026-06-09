using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireDebugScenarioRunner : MonoBehaviour
    {
        [Header("Debug Scenario")]
        [SerializeField] private bool replaceLevelStartDeal;
        [SerializeField] private SolitaireDebugScenarioId scenario = SolitaireDebugScenarioId.ValidFourCardMerge;
        [TextArea(3, 6)]
        [SerializeField] private string scenarioInstructions;

        private SolitaireDeckController _deckController;

        public bool ShouldReplaceLevelStartDeal =>
            isActiveAndEnabled && replaceLevelStartDeal && scenario != SolitaireDebugScenarioId.None;

        public SolitaireDebugScenarioId Scenario => scenario;

        private void Awake()
        {
            _deckController = GetComponent<SolitaireDeckController>();
        }

        private void OnValidate()
        {
            RefreshInstructions();
        }

        public bool TryStartDebugDeal()
        {
            if (!ShouldReplaceLevelStartDeal)
                return false;

            ApplyScenario(scenario);
            return true;
        }

        [ContextMenu("Apply Selected Scenario (Play Mode)")]
        public void ApplySelectedScenarioFromContextMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SolitaireDebugScenario] Enter Play Mode first.");
                return;
            }

            if (scenario == SolitaireDebugScenarioId.None)
            {
                Debug.LogWarning("[SolitaireDebugScenario] Select a scenario first.");
                return;
            }

            ApplyScenario(scenario);
        }

        public void ApplyScenario(SolitaireDebugScenarioId scenarioId)
        {
            if (SolitaireDebugScenarioApplier.IsFlowScenario(scenarioId))
            {
                SolitaireDebugScenarioApplier.ApplyFlowScenario(scenarioId);
                Debug.Log($"[SolitaireDebugScenario] Applied flow scenario {scenarioId}.\n{SolitaireDebugScenarioApplier.GetInstructions(scenarioId)}");
                return;
            }

            if (_deckController == null)
                _deckController = GetComponent<SolitaireDeckController>();

            if (_deckController == null)
            {
                Debug.LogError("[SolitaireDebugScenario] Missing SolitaireDeckController reference.");
                return;
            }

            _deckController.StartDebugScenario(scenarioId);
            Debug.Log($"[SolitaireDebugScenario] Applied {scenarioId}.\n{SolitaireDebugScenarioApplier.GetInstructions(scenarioId)}");
        }

        private void RefreshInstructions()
        {
            scenarioInstructions = scenario == SolitaireDebugScenarioId.None
                ? "Senaryo seç."
                : SolitaireDebugScenarioApplier.GetInstructions(scenario);
        }
    }
}
