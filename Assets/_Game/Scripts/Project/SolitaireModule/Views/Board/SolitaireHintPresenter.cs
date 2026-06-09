using System.Collections;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class SolitaireHintPresenter : MonoBehaviour
    {
        [SerializeField] private Color targetHighlightColor = new Color(0.25f, 1f, 0.65f, 1f);
        [SerializeField] private float visibleDuration = 1.25f;

        private CardView _hintCard;
        private SolitaireSlotAnchor _hintSlot;
        private Coroutine _clearRoutine;

        private void OnEnable()
        {
            EventManager.SolitaireEvents.HintShown += HandleHintShown;
            EventManager.SolitaireEvents.DealStarted += Clear;
            EventManager.SolitaireEvents.CardDropSucceeded += Clear;
            EventManager.SolitaireEvents.StockDrawClicked += Clear;
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.HintShown -= HandleHintShown;
            EventManager.SolitaireEvents.DealStarted -= Clear;
            EventManager.SolitaireEvents.CardDropSucceeded -= Clear;
            EventManager.SolitaireEvents.StockDrawClicked -= Clear;
            Clear();
        }

        private void HandleHintShown(SolitaireHint hint)
        {
            Clear();

            if (!hint.IsValid)
                return;

            if (hint.Move.StartCardId >= 0 &&
                SolitaireFeatureRegistration.TryGetRegisteredCard(hint.Move.StartCardId, out CardView card))
            {
                _hintCard = card;
                _hintCard.PlayPressedFeedback();
            }

            if (SolitaireFeatureRegistration.TryGetRegisteredSlot(hint.Move.Target, out SolitaireSlotAnchor slot))
            {
                _hintSlot = slot;
                _hintSlot.SetHighlight(true, targetHighlightColor);
            }

            _clearRoutine = StartCoroutine(ClearAfterDelay());
        }

        private IEnumerator ClearAfterDelay()
        {
            yield return new WaitForSeconds(visibleDuration);
            Clear();
        }

        private void Clear()
        {
            if (_clearRoutine != null)
            {
                StopCoroutine(_clearRoutine);
                _clearRoutine = null;
            }

            if (_hintCard != null)
            {
                _hintCard.ResetFeedback();
                _hintCard = null;
            }

            if (_hintSlot != null)
            {
                _hintSlot.SetHighlight(false, targetHighlightColor);
                _hintSlot = null;
            }
        }
    }
}
