using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.RunTime
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
