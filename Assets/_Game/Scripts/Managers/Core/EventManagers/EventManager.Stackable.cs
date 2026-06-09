using _Game.Scripts.Template.GlobalProviders.Interactable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Stacking;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class StackableEvents
        {
            public static UnityAction<StackableData> Stack;
            public static UnityAction<StackableData> Unstack;
        }
    }
}