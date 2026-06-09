using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Buttons
{
    public sealed class SolitaireUndoButton : ButtonBase
    {
        [SerializeField] private SolitaireDeckController deckController;
        [SerializeField] private Graphic[] disabledVisuals;

        private bool _isSubscribed;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelStart += HandleLevelStart;
            EventManager.SolitaireEvents.ControllerHostReady += HandleControllerHostReady;
            TryResolveDeckController();
            SubscribeIfReady();
        }

        private void Start()
        {
            TryResolveDeckController();
            SubscribeIfReady();
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelStart -= HandleLevelStart;
            EventManager.SolitaireEvents.ControllerHostReady -= HandleControllerHostReady;
            Unsubscribe();
        }

        protected override void OnClicked()
        {
            TryResolveDeckController();

            if (deckController == null)
                return;

            deckController.TryUndo();
        }

        private void HandleLevelStart()
        {
            TryResolveDeckController();
            SubscribeIfReady();
        }

        private void HandleControllerHostReady(SolitaireModuleControllerBundle bundle)
        {
            if (bundle == null)
                return;

            deckController = bundle.DeckController;
            SubscribeIfReady();
        }

        private void TryResolveDeckController()
        {
            if (deckController != null)
                return;

            if (SolitaireFeatureRegistration.TryGetControllerHost(out SolitaireModuleControllerBundle bundle, out _))
                deckController = bundle.DeckController;
        }

        private void SubscribeIfReady()
        {
            if (deckController == null)
            {
                RefreshAvailability(false);
                return;
            }

            if (_isSubscribed)
                return;

            deckController.UndoAvailabilityChanged += RefreshAvailability;
            _isSubscribed = true;
            RefreshAvailability(deckController.CanUndo);
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || deckController == null)
                return;

            deckController.UndoAvailabilityChanged -= RefreshAvailability;
            _isSubscribed = false;
        }

        private void RefreshAvailability(bool canUndo)
        {
            if (TargetButton != null)
                TargetButton.interactable = true;

            if (disabledVisuals == null)
                return;

            for (int i = 0; i < disabledVisuals.Length; i++)
            {
                Graphic visual = disabledVisuals[i];
                if (visual == null)
                    continue;

                Color color = visual.color;
                color.a = 1f;
                visual.color = color;
            }
        }
    }
}
