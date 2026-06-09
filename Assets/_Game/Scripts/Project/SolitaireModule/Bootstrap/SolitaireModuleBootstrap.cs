using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    /// <summary>
    /// Composition root for SolitaireRoot. Owns config only; scene objects self-register through EventManager.
    /// </summary>
    public sealed class SolitaireModuleBootstrap : MonoBehaviour
    {
        [SerializeField] private SolitaireDeckConfigSO deckConfig;

        private SolitaireModuleRuntimeBootstrap _bootstrap;
        private SolitaireModuleControllerBundle _controllers;
        private bool _hasStartedDeal;

        private void Start()
        {
            EnsureRuntimeInitialized();
        }

        private void EnsureRuntimeInitialized()
        {
            if (_bootstrap != null)
                return;

            if (!SolitaireFeatureRegistration.TryCreateViewRegistry(out SolitaireViewRegistry registry, out string error))
                throw new System.InvalidOperationException(error);

            if (!SolitaireFeatureRegistration.TryGetControllerHost(out _controllers, out error))
                throw new System.InvalidOperationException(error);

            if (deckConfig == null || !deckConfig.Validate(out error))
                throw new System.InvalidOperationException(error);

            _bootstrap = new SolitaireModuleRuntimeBootstrap(deckConfig, registry, _controllers);
            _bootstrap.Initialize(this);
        }

        private void OnEnable()
        {
            if (!_hasStartedDeal)
                _bootstrap?.SetBoardVisible(false);
        }

        public void StartDeal()
        {
            EnsureRuntimeInitialized();
            EnsureDealStarted();

            if (_controllers.DebugScenarioRunner != null && _controllers.DebugScenarioRunner.TryStartDebugDeal())
                return;

            _controllers.DeckController.StartNewDeal();
        }

        public bool Validate(out string error)
        {
            if (deckConfig == null)
            {
                error = "SolitaireDeckConfigSO is missing.";
                return false;
            }

            if (!deckConfig.Validate(out error))
                return false;

            if (!SolitaireFeatureRegistration.TryCreateViewRegistry(out _, out error))
                return false;

            if (!SolitaireFeatureRegistration.TryGetControllerHost(out _, out error))
                return false;

            error = string.Empty;
            return true;
        }

        public void ApplyDebugScenarioInPlayMode(SolitaireDebugScenarioId scenarioId)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SolitaireModuleBootstrap] Enter Play Mode first.");
                return;
            }

            if (scenarioId == SolitaireDebugScenarioId.None)
            {
                Debug.LogWarning("[SolitaireModuleBootstrap] Select a scenario first.");
                return;
            }

            if (SolitaireDebugScenarioApplier.IsFlowScenario(scenarioId))
            {
                SolitaireDebugScenarioApplier.ApplyFlowScenario(scenarioId);
                Debug.Log($"[SolitaireModuleBootstrap] Flow debug scenario applied: {scenarioId}");
                return;
            }

            EnsureRuntimeInitialized();
            EnsureDealStarted();

            if (_controllers.DeckController == null)
            {
                Debug.LogError("[SolitaireModuleBootstrap] DeckController is missing; debug scenario was not applied.");
                return;
            }

            _controllers.DeckController.StartDebugScenario(scenarioId);
            Debug.Log($"[SolitaireModuleBootstrap] Board debug scenario applied: {scenarioId}");
        }

        private void EnsureDealStarted()
        {
            if (_hasStartedDeal)
                return;

            _hasStartedDeal = true;
            _bootstrap?.SetBoardVisible(true);
        }
    }
}
