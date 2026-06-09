using _Game.Scripts.Project.SolitaireModule.Controllers;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireModuleControllerBundle
    {
        public SolitaireDeckController DeckController { get; }
        public SolitaireInputController InputController { get; }
        public SolitaireLayoutController LayoutController { get; }
        public SolitairePointerInputSource PointerInputSource { get; }
        public SolitaireHapticFeedbackProvider HapticFeedbackProvider { get; }
        public SolitaireLevelStartBridge LevelStartBridge { get; }
        public SolitaireWinBridge WinBridge { get; }
        public SolitaireDebugScenarioRunner DebugScenarioRunner { get; }

        private SolitaireModuleControllerBundle(
            SolitaireDeckController deckController,
            SolitaireInputController inputController,
            SolitaireLayoutController layoutController,
            SolitairePointerInputSource pointerInputSource,
            SolitaireHapticFeedbackProvider hapticFeedbackProvider,
            SolitaireLevelStartBridge levelStartBridge,
            SolitaireWinBridge winBridge,
            SolitaireDebugScenarioRunner debugScenarioRunner)
        {
            DeckController = deckController;
            InputController = inputController;
            LayoutController = layoutController;
            PointerInputSource = pointerInputSource;
            HapticFeedbackProvider = hapticFeedbackProvider;
            LevelStartBridge = levelStartBridge;
            WinBridge = winBridge;
            DebugScenarioRunner = debugScenarioRunner;
        }

        public static SolitaireModuleControllerBundle FromHost(GameObject host)
        {
            if (host == null)
                throw new System.ArgumentNullException(nameof(host));

            return new SolitaireModuleControllerBundle(
                Require(host.GetComponent<SolitaireDeckController>(), nameof(SolitaireDeckController)),
                Require(host.GetComponent<SolitaireInputController>(), nameof(SolitaireInputController)),
                Require(host.GetComponent<SolitaireLayoutController>(), nameof(SolitaireLayoutController)),
                Require(host.GetComponent<SolitairePointerInputSource>(), nameof(SolitairePointerInputSource)),
                Require(host.GetComponent<SolitaireHapticFeedbackProvider>(), nameof(SolitaireHapticFeedbackProvider)),
                Require(host.GetComponent<SolitaireLevelStartBridge>(), nameof(SolitaireLevelStartBridge)),
                Require(host.GetComponent<SolitaireWinBridge>(), nameof(SolitaireWinBridge)),
                host.GetComponent<SolitaireDebugScenarioRunner>());
        }

        private static T Require<T>(T component, string componentName) where T : Component
        {
            return component != null
                ? component
                : throw new System.InvalidOperationException($"{componentName} is missing on ControllerHost.");
        }
    }
}
