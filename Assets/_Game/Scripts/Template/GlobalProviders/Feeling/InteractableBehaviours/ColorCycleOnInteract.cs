using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Interactable;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.Generic
{
    public class ColorCycleOnInteract : ColorCycleProvider, IInteractableAction
    {
        #region Public Methods
        public void OnInteract()
        {
            StartColorCycle();
        }

        #endregion
    }
}