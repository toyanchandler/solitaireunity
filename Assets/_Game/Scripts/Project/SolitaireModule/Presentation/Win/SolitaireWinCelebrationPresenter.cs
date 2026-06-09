using System;
using System.Collections;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitaireWinCelebrationPresenter
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private Action<float> _lockInputForAnimation;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            Action<float> lockInputForAnimation)
        {
            _config = config;
            _context = context;
            _lockInputForAnimation = lockInputForAnimation;
        }

        public IEnumerator PlayRoutine(Action onCompleted)
        {
            _lockInputForAnimation?.Invoke(_config.WinCelebrationDuration);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                _context.ViewRegistry.Foundations[i]?.PlayWinPulse(_config);

            float popDuration = Mathf.Max(0.12f, _config.WinCardStaggerDelay * 4f);
            int launchedCards = 0;

            for (int foundationIndex = 0; foundationIndex < SolitaireCardUtility.FoundationCount; foundationIndex++)
            {
                FixedCardPileState pile = _context.BoardState.GetPile(new PileRef(SolitairePileType.Foundation, foundationIndex));

                for (int cardIndex = 0; cardIndex < pile.Count; cardIndex++)
                {
                    CardView card = _context.ViewRegistry.GetCard(pile[cardIndex]);
                    card.PlayWinPop(_config.WinCardPopHeight, popDuration);
                    launchedCards++;
                    yield return new WaitForSeconds(_config.WinCardStaggerDelay);
                }
            }

            if (launchedCards == 0)
                yield return new WaitForSeconds(0.25f);
            else
                yield return new WaitForSeconds(popDuration);

            onCompleted?.Invoke();
        }
    }
}
