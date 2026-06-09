using UnityEngine;

namespace _Game.Scripts.RuntimeState
{
    public abstract class ResettableRuntimeObject : ScriptableObject
    {
        [SerializeField] private bool _resetOnEnable = true;

        private void OnEnable()
        {
            if (_resetOnEnable)
            {
                ResetRuntimeState();
            }
        }

        public abstract void ResetRuntimeState();
    }
}

/*
Implementation note:
If your project disables domain reload, add a central runtime reset service that calls ResetRuntimeState() for all registered runtime objects at session start.
*/
