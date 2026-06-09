using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Interactable;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public class PunchScaleOnInteract : PunchScaleProvider, IInteractableAction
    {
        #region Public Methods

        public void OnInteract()
        {
            PunchScale();
        }

        #endregion
    }
}