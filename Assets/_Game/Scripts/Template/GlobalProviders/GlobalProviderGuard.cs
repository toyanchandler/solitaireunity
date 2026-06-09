using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders
{
    internal static class GlobalProviderGuard
    {
        public static void Require(UnityEngine.Object value, Component owner, string fieldName)
        {
            if (value != null) return;

            throw new InvalidOperationException($"{owner.GetType().Name} on {owner.name} requires {fieldName}.");
        }

        public static TAction[] BuildActionCache<TAction>(
            Component owner,
            IReadOnlyList<MonoBehaviour> actionComponents) where TAction : class
        {
            if (actionComponents == null || actionComponents.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{owner.GetType().Name} on {owner.name} requires at least one {typeof(TAction).Name} component.");
            }

            var actions = new List<TAction>(actionComponents.Count);

            for (var i = 0; i < actionComponents.Count; i++)
            {
                var actionComponent = actionComponents[i];
                if (actionComponent == null)
                {
                    throw new InvalidOperationException(
                        $"{owner.GetType().Name} on {owner.name} has an empty action slot at index {i}.");
                }

                if (actionComponent is not TAction action)
                {
                    throw new InvalidOperationException(
                        $"{actionComponent.GetType().Name} on {actionComponent.name} must implement {typeof(TAction).Name}.");
                }

                actions.Add(action);
            }

            return actions.ToArray();
        }

#if UNITY_EDITOR
        public static MonoBehaviour[] CollectChildActions<TAction>(Component owner) where TAction : class
        {
            var components = owner.GetComponentsInChildren<MonoBehaviour>(true);
            var actions = new List<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null || component == owner) continue;
                if (component is TAction)
                {
                    actions.Add(component);
                }
            }

            return actions.ToArray();
        }
#endif
    }
}
