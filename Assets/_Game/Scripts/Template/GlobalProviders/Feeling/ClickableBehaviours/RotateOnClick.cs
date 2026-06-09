using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Input;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.ClickableBehaviours
{
    public class RotateOnClick : ObjectRotateProvider, IClickableAction
    {
        #region Public Methods

        public void OnClickDown()
        {
            StartRotateCoroutine();
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