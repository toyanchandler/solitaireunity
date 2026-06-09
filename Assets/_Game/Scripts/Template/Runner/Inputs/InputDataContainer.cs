using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Game.Scripts.Template.Runner.Inputs
{
    public sealed class InputDataContainer
    {
        #region Data Structures

        public struct InputInternalState
        {
            public float CurrentHorizontalInput;
            public float LastHorizontalInput;
            public float InertiaTimer;
            public float Sensitivity;
        }

        [Serializable]
        public struct InputSettings
        {
            public float BaseSensitivity;
            public float minRangeX;
            public float maxRangeX;
        }

        [Serializable]
        public struct InputConstants
        {
            public float Damping;
            public float InertiaDuration;
            public float InputLag;
        }
        
        [Serializable]
        public struct TransformSettings
        {
            public Transform _targetTransform;
        }

        #endregion
    }
}
