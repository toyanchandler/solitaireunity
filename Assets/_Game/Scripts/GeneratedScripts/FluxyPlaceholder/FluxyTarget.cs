using UnityEngine;

namespace Fluxy
{
    [DisallowMultipleComponent]
    public sealed class FluxyTarget : MonoBehaviour
    {
        public Material splatMaterial;
        public int rateOverSteps = 1;
        public int rateOverTime;
        public float rateOverDistance;
        public bool overridePosition;
        public Vector2 position;
        public float positionRandomness;
        public bool overrideRotation;
        public float rotation;
        public float rotationRandomness;
        public bool scaleWithDistance;
        public bool scaleWithTransform = true;
        public Vector2 scale = Vector2.one;
        public float scaleRandomness;
        public float velocityWeight = 1;
        public Texture2D velocityTexture;
        public float maxRelativeVelocity = 8;
        public Vector3 velocityScale = Vector3.one;
        public float maxRelativeAngularVelocity = 12;
        public float angularVelocityScale = 1;
        public Vector3 force;
        public float torque;
        public float densityWeight = 1;
        public Texture2D densityTexture;
        public int srcBlend;
        public int dstBlend;
        public int blendOp;
        public Color color = Color.white;
        public Texture2D noiseTexture;
        public float densityNoise;
        public float densityNoiseOffset;
        public float densityNoiseTiling = 1;
        public float velocityNoise;
        public float velocityNoiseOffset;
        public float velocityNoiseTiling = 1;
    }
}
