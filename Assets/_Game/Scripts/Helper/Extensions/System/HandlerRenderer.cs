using Handler.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace Handler.Extensions
{
    public static class HandlerRenderer
    {
        private static void SetActiveShadowCastingMode(this Renderer renderer, bool value)
        {
            if (renderer.shadowCastingMode != ShadowCastingMode.Off)
            {
                renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }
        
        private static void SetActiveReceiveShadows(this Renderer renderer, bool value)
        {
            if (renderer.receiveShadows != value)
            {
                renderer.receiveShadows = value;
            }
        }
        
        private static void SetActiveLightProbeUsage(this Renderer renderer, bool value)
        {
            if (renderer.lightProbeUsage != LightProbeUsage.Off)
            {
                renderer.lightProbeUsage = value ? LightProbeUsage.BlendProbes : LightProbeUsage.Off;
            }
        }
        
        public static void OptimizeRenderer(this Renderer renderer, bool shadow, bool occlusionCulling)
        {
            renderer.SetActiveShadowCastingMode(shadow);
            renderer.SetActiveReceiveShadows(shadow);
            renderer.SetActiveLightProbeUsage(occlusionCulling);
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = occlusionCulling;
        }
    }
}