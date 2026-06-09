using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Input;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.ClickableBehaviours
{
    public class ChangePositionOnClick : TransformUpdateProvider, IClickableAction
    {
        #region Public Methods

        public void OnClickDown()
        {
            ChangePosition();
        }

        public void OnClickHold()
        {
        }

        public void OnClickUp()
        {
        }

        #endregion
        
    }
}