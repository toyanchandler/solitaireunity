using System;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireModuleRuntimeBootstrap
    {
        private readonly SolitaireDeckConfigSO _config;
        private readonly SolitaireViewRegistry _registry;
        private readonly SolitaireModuleControllerBundle _controllers;

        public SolitaireRuntimeContext Context { get; private set; }

        public SolitaireModuleRuntimeBootstrap(
            SolitaireDeckConfigSO config,
            SolitaireViewRegistry registry,
            SolitaireModuleControllerBundle controllers)
        {
            _config = config;
            _registry = registry;
            _controllers = controllers;
        }

        public void Initialize(SolitaireModuleBootstrap bootstrap)
        {
            Context = new SolitaireRuntimeContext(new SolitaireBoardState(), _registry);

            var moveResolver = new SolitaireMoveResolver();
            var moveExecutor = new SolitaireMoveExecutor(moveResolver);

            _controllers.HapticFeedbackProvider.Initialize(_config);
            _controllers.LevelStartBridge.Initialize(bootstrap);
            _controllers.WinBridge.Initialize(_controllers.DeckController);
            _controllers.LayoutController.Initialize(_config, Context);
            _controllers.DeckController.Initialize(
                _config,
                Context,
                moveResolver,
                moveExecutor,
                _controllers.LayoutController,
                _controllers.HapticFeedbackProvider);
            _controllers.InputController.Initialize(
                _config,
                Context,
                _controllers.DeckController,
                _controllers.PointerInputSource,
                _controllers.HapticFeedbackProvider);

            SetBoardVisible(false);
        }

        public void SetBoardVisible(bool isVisible)
        {
            // Slots stay active — they are static board anchors. Hiding them breaks hit tests and
            // prevents child cards from running coroutines under inactive parents.
            SetActive(_registry.Cards, isVisible);
        }

        private static void SetActive(CardView[] cards, bool isVisible)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                CardView card = cards[i];

                if (card != null)
                    card.gameObject.SetActive(isVisible);
            }
        }
    }
}
