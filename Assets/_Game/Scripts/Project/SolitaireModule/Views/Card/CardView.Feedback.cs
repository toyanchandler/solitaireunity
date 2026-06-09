using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed partial class CardView
    {
        public void PlayPressedFeedback()
        {
            if (CardViewLogic.Guard.ShouldSkipPressedFeedback(IsPresenting, _isDragVisualActive))
                return;

            transform.localScale = CardViewLogic.Feedback.ResolvePressedScale(_homeScale);
        }

        public void ResetFeedback()
        {
            if (CardViewLogic.Guard.ShouldSkipResetFeedback(IsPresenting))
                return;

            transform.localScale = _homeScale;
            CardViewLogic.SpriteRendererOps.SetColor(cardRenderer, _homeColor);
        }
    }
}
