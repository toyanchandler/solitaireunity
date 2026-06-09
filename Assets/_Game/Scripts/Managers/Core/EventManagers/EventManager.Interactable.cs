
using _Game.Scripts.Template.GlobalProviders.Interactable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class InteractableEvents
        {
            public static UnityAction<InteractableData> Interact;
            
            public static UnityAction<GateInteractableData> GateInteract;
        }
    }
}
