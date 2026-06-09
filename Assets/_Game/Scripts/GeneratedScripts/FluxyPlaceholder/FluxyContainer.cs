using System;
using UnityEngine;

namespace Fluxy
{
    [DisallowMultipleComponent]
    public sealed class FluxyContainer : MonoBehaviour
    {
        [Serializable]
        public struct EdgeFalloff
        {
            public float densityEdgeWidth;
            public float densityFalloffRate;
            public float velocityEdgeWidth;
            public float velocityFalloffRate;
        }

        [Serializable]
        public struct Boundaries
        {
            public int horizontalBoundary;
            public int verticalBoundary;
        }

        public int containerShape;
        public Vector2Int subdivisions = Vector2Int.one;
        public Mesh customMesh;
        public Vector3 size = Vector3.one;
        public int lookAtMode;
        public Transform lookAt;
        public Camera projectFrom;
        public Texture2D clearTexture;
        public Color clearColor = Color.clear;
        public Texture2D surfaceNormals;
        public Vector2 normalTiling = Vector2.one;
        public float normalScale = 1;
        public EdgeFalloff edgeFalloff;
        public Boundaries boundaries;
        public float velocityScale;
        public float accelerationScale;
        public Vector2 positionOffset;
        public Vector3 gravity;
        public Vector3 externalForce;
        public Light lightSource;
        public FluxyTarget[] targets = Array.Empty<FluxyTarget>();
        public float pressure = 1;
        public float viscosity;
        public float turbulence;
        public float adhesion;
        public float surfaceTension;
        public float buoyancy;
        public Vector4 dissipation;
        [SerializeField] private FluxySolver m_Solver;

        public FluxySolver solver
        {
            get => m_Solver;
            set => m_Solver = value;
        }
    }
}
