using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Input;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.ClickableBehaviours
{
    public class PunchScaleOnClick : PunchScaleProvider, IClickableAction
    {
        public void OnClickDown()
        {
            PunchScale();
        }

        public void OnClickHold()
        {
        }

        public void OnClickUp()
        {
        }
    }
}