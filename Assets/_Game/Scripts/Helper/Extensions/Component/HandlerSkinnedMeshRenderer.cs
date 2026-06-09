using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerSkinnedMeshRenderer
    {
        public static void SetBlendShapeWeightSafe(this SkinnedMeshRenderer self, int index, float value)
        {
            if (self.GetBlendShapeWeight(index) != value)
            {
                self.SetBlendShapeWeight(index, value);
            }
        }
        
        public static void SetMaterialSafe(this SkinnedMeshRenderer self, Material material)
        {
            if (self.material != material)
            {
                self.material = material;
            }
        }
    }
}