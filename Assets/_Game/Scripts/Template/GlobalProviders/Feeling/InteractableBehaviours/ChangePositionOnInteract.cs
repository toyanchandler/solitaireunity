using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Interactable;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public class ChangePositionOnInteract : TransformUpdateProvider, IInteractableAction
    {
        #region Public Methods

        public void OnInteract()
        {
            ChangePosition();
        }

        #endregion
    }
}