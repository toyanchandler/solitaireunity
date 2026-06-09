using System;
using Handler.Extensions;
using _Game.Scripts.Template.GlobalProviders;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Input
{
    public class ClickableObject : MonoBehaviour, IClickable
    {
        #region Serialized Fields

        [SerializeField] private bool canClick = true;

        [SerializeField] private MonoBehaviour[] actionComponents = Array.Empty<MonoBehaviour>();

        [ShowInInspector, ReadOnly] private IClickableAction[] actions = Array.Empty<IClickableAction>();

        [SerializeField] private ClickData clickData;

        #endregion

        #region Private Fields

        private bool isInitialized;

        #endregion

        #region Public Properties

        private bool CanClick => canClick;

        #endregion

        #region Private Methods

        private void EnsureInitialized()
        {
            if (isInitialized) return;

            actions = GlobalProviderGuard.BuildActionCache<IClickableAction>(this, actionComponents);
            isInitialized = true;
        }

        private void InvokeActions(Action<IClickableAction> actionInvoker)
        {
            EnsureInitialized();

            if (!CanClick || actions == null || actions.Length == 0) return;

            foreach (var action in actions)
            {
                actionInvoker(action);
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            EnsureInitialized();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            actionComponents = GlobalProviderGuard.CollectChildActions<IClickableAction>(this);
        }
#endif

        #endregion

        #region IClickable Implementation

        public void OnClickedDown()
        {
            InvokeActions(action => action.OnClickDown());
        }

        public void OnClickedHold()
        {
            InvokeActions(action => action.OnClickHold());
        }

        public void OnClickedUp()
        {
            InvokeActions(action => action.OnClickUp());
        }

        #endregion
    }
}
