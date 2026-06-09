using System;
using UnityEngine;

namespace Fluxy
{
    [DisallowMultipleComponent]
    public sealed class FluxySolver : MonoBehaviour
    {
        [Flags]
        public enum ReadbackMode
        {
            None = 0,
            Velocity = 1,
            Density = 2,
            All = ~0
        }

        public ScriptableObject storage;
        public int desiredResolution = 256;
        public int densitySupersampling = 1;
        public bool disposeWhenCulled = true;
        public ReadbackMode readable = ReadbackMode.All;
        public Material simulationMaterial;
        public float maxTimestep = 0.008f;
        public int maxSteps = 1;
        public int pressureSolver = 1;
        public int pressureIterations = 3;
    }
}
