using System;
using _Game.Scripts.Template.GlobalProviders.Interactable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class CollectableEvents
        {
            public static UnityAction<CollectableData> Collect;

            public static UnityAction<int, Action<bool>> TryToBuyWithCollectable;
            
            public static UnityAction<CollectableData> UICollectAnimation;
        }
    }
}